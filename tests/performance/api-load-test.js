import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';

// Custom metrics
export const errorRate = new Rate('errors');
export const apiResponseTime = new Trend('api_response_time');

// Test configuration
export const options = {
  stages: [
    { duration: '30s', target: 10 },   // Ramp up to 10 users
    { duration: '1m', target: 20 },    // Stay at 20 users
    { duration: '30s', target: 50 },   // Ramp up to 50 users
    { duration: '1m', target: 50 },    // Stay at 50 users
    { duration: '30s', target: 0 },    // Ramp down to 0 users
  ],
  thresholds: {
    'http_req_duration': ['p(95)<500'], // 95% of requests should be below 500ms
    'http_req_failed': ['rate<0.1'],    // Error rate should be less than 10%
    'errors': ['rate<0.1'],             // Custom error rate should be less than 10%
  },
};

// Base URL configuration
const BASE_URL = __ENV.API_BASE_URL || 'http://localhost:5253';

// Test data
const testUser = {
  username: 'testuser_' + Math.random().toString(36).substr(2, 9),
  email: 'test_' + Math.random().toString(36).substr(2, 9) + '@example.com',
  password: 'TestPassword123!',
  confirmPassword: 'TestPassword123!',
  fullName: 'Test User Performance'
};

let authToken = '';

export function setup() {
  // Health check
  const healthResponse = http.get(`${BASE_URL}/api/rbac/health`);
  console.log(`Health check status: ${healthResponse.status}`);
  
  return { baseUrl: BASE_URL };
}

export default function(data) {
  // Test suite: Authentication Flow
  testAuthenticationFlow();
  
  // Test suite: Public API endpoints
  testPublicEndpoints();
  
  // Test suite: Protected API endpoints (if authenticated)
  if (authToken) {
    testProtectedEndpoints();
  }
  
  sleep(1);
}

function testAuthenticationFlow() {
  const group = 'Authentication Flow';
  
  // Test user registration
  const registerPayload = JSON.stringify(testUser);
  const registerParams = {
    headers: { 'Content-Type': 'application/json' },
    tags: { name: 'register' },
  };
  
  const registerResponse = http.post(
    `${BASE_URL}/api/auth/register`, 
    registerPayload, 
    registerParams
  );
  
  const registerSuccess = check(registerResponse, {
    [`${group} - Register: Status is 201 or 400`]: (r) => r.status === 201 || r.status === 400,
    [`${group} - Register: Response time < 2s`]: (r) => r.timings.duration < 2000,
  });
  
  apiResponseTime.add(registerResponse.timings.duration);
  errorRate.add(!registerSuccess);
  
  // If registration successful, extract token
  if (registerResponse.status === 201) {
    const registerData = JSON.parse(registerResponse.body);
    if (registerData.isSuccess && registerData.data && registerData.data.accessToken) {
      authToken = registerData.data.accessToken;
    }
  }
  
  // Test user login (if registration failed, try with existing user)
  if (!authToken) {
    const loginPayload = JSON.stringify({
      username: 'admin',
      password: 'password123'
    });
    
    const loginResponse = http.post(
      `${BASE_URL}/api/auth/login`,
      loginPayload,
      registerParams
    );
    
    const loginSuccess = check(loginResponse, {
      [`${group} - Login: Status is 200 or 401`]: (r) => r.status === 200 || r.status === 401,
      [`${group} - Login: Response time < 1s`]: (r) => r.timings.duration < 1000,
    });
    
    apiResponseTime.add(loginResponse.timings.duration);
    errorRate.add(!loginSuccess);
  }
}

function testPublicEndpoints() {
  const group = 'Public Endpoints';
  
  // Test public profile endpoint
  const publicProfileResponse = http.get(
    `${BASE_URL}/api/personalprofiles/public`,
    { tags: { name: 'public-profiles' } }
  );
  
  const publicProfileSuccess = check(publicProfileResponse, {
    [`${group} - Public Profiles: Status is 200`]: (r) => r.status === 200,
    [`${group} - Public Profiles: Response time < 500ms`]: (r) => r.timings.duration < 500,
    [`${group} - Public Profiles: Has JSON response`]: (r) => {
      try {
        JSON.parse(r.body);
        return true;
      } catch {
        return false;
      }
    },
  });
  
  apiResponseTime.add(publicProfileResponse.timings.duration);
  errorRate.add(!publicProfileSuccess);
  
  // Test public skills endpoint
  const publicSkillsResponse = http.get(
    `${BASE_URL}/api/skills/public`,
    { tags: { name: 'public-skills' } }
  );
  
  const publicSkillsSuccess = check(publicSkillsResponse, {
    [`${group} - Public Skills: Status is 200`]: (r) => r.status === 200,
    [`${group} - Public Skills: Response time < 500ms`]: (r) => r.timings.duration < 500,
  });
  
  apiResponseTime.add(publicSkillsResponse.timings.duration);
  errorRate.add(!publicSkillsSuccess);
}

function testProtectedEndpoints() {
  const group = 'Protected Endpoints';
  const headers = {
    headers: {
      'Authorization': `Bearer ${authToken}`,
      'Content-Type': 'application/json'
    },
    tags: { name: 'protected' }
  };
  
  // Test protected users endpoint
  const usersResponse = http.get(`${BASE_URL}/api/users`, headers);
  
  const usersSuccess = check(usersResponse, {
    [`${group} - Users: Status is 200 or 403`]: (r) => r.status === 200 || r.status === 403,
    [`${group} - Users: Response time < 1s`]: (r) => r.timings.duration < 1000,
  });
  
  apiResponseTime.add(usersResponse.timings.duration);
  errorRate.add(!usersSuccess);
  
  // Test protected RBAC health endpoint
  const rbacHealthResponse = http.get(`${BASE_URL}/api/rbac/health`, headers);
  
  const rbacHealthSuccess = check(rbacHealthResponse, {
    [`${group} - RBAC Health: Status is 200 or 403`]: (r) => r.status === 200 || r.status === 403,
    [`${group} - RBAC Health: Response time < 500ms`]: (r) => r.timings.duration < 500,
  });
  
  apiResponseTime.add(rbacHealthResponse.timings.duration);
  errorRate.add(!rbacHealthSuccess);
}

export function teardown(data) {
  // Cleanup if needed
  console.log('Performance test completed');
}