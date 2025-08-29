using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PersonalManagerAPI.Data;
using PersonalManagerAPI.Services;
using PersonalManagerAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure data access mode (Entity Framework or JSON fallback)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var useEntityFramework = !string.IsNullOrEmpty(connectionString) && 
                         builder.Configuration.GetValue<bool>("UseEntityFramework", true);

if (useEntityFramework && !string.IsNullOrEmpty(connectionString))
{
    // Configure Entity Framework with MariaDB
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mysqlOptions =>
        {
            mysqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
        
        // Enable sensitive data logging in development
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    });
}
else
{
    // Configure fallback mode - use InMemory database for DI compatibility (requires Microsoft.EntityFrameworkCore.InMemory)
    // For development without external database
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        // Use InMemory database for DI compatibility when no external database is available
        options.UseInMemoryDatabase("FallbackDatabase");
    });
}

// Add JSON Data Service for development (fallback)
builder.Services.AddScoped<JsonDataService>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(PersonalManagerAPI.Mappings.MappingProfile));

// Add Business Service Layer with dual mode support
if (useEntityFramework && !string.IsNullOrEmpty(connectionString))
{
    // Entity Framework mode
    builder.Services.AddScoped<PersonalManagerAPI.Services.Implementation.UserServiceEF>();
    builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.IUserService>(provider =>
        provider.GetRequiredService<PersonalManagerAPI.Services.Implementation.UserServiceEF>());
    
    // TODO: Add other EF service implementations when available
    // For now, use JSON services as fallback for other services
    builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.IPersonalProfileService, PersonalManagerAPI.Services.Implementation.PersonalProfileService>();
    builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.IEducationService, PersonalManagerAPI.Services.Implementation.EducationService>();
    builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.ISkillService, PersonalManagerAPI.Services.Implementation.SkillService>();
    builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.IWorkExperienceService, PersonalManagerAPI.Services.Implementation.WorkExperienceService>();
    builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.IPortfolioService, PersonalManagerAPI.Services.Implementation.PortfolioService>();
    builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.ICalendarEventService, PersonalManagerAPI.Services.Implementation.CalendarEventService>();
    builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.ITodoItemService, PersonalManagerAPI.Services.Implementation.TodoItemService>();
    builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.IWorkTaskService, PersonalManagerAPI.Services.Implementation.WorkTaskService>();
    builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.IBlogPostService, PersonalManagerAPI.Services.Implementation.BlogPostService>();
    builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.IGuestBookService, PersonalManagerAPI.Services.Implementation.GuestBookService>();
    builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.IContactMethodService, PersonalManagerAPI.Services.Implementation.ContactMethodService>();
}
else
{
    // JSON Data Service mode (fallback)
    builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.IUserService, PersonalManagerAPI.Services.Implementation.UserService>();
    builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.IPersonalProfileService, PersonalManagerAPI.Services.Implementation.PersonalProfileService>();
    builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.IEducationService, PersonalManagerAPI.Services.Implementation.EducationService>();
    builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.ISkillService, PersonalManagerAPI.Services.Implementation.SkillService>();
    builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.IWorkExperienceService, PersonalManagerAPI.Services.Implementation.WorkExperienceService>();
    builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.IPortfolioService, PersonalManagerAPI.Services.Implementation.PortfolioService>();
    builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.ICalendarEventService, PersonalManagerAPI.Services.Implementation.CalendarEventService>();
    builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.ITodoItemService, PersonalManagerAPI.Services.Implementation.TodoItemService>();
    builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.IWorkTaskService, PersonalManagerAPI.Services.Implementation.WorkTaskService>();
    builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.IBlogPostService, PersonalManagerAPI.Services.Implementation.BlogPostService>();
    builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.IGuestBookService, PersonalManagerAPI.Services.Implementation.GuestBookService>();
    builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.IContactMethodService, PersonalManagerAPI.Services.Implementation.ContactMethodService>();
}

// Service Factory was removed as it was not being used

// Add File Services for media handling
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IFileSecurityService, FileSecurityService>();
builder.Services.AddScoped<IFileQuarantineService, FileQuarantineService>();

// Add Authentication Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<PersonalManagerAPI.Services.Interfaces.ITokenBlacklistService, PersonalManagerAPI.Services.Implementation.TokenBlacklistService>();

// Add Session Management Services
builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.IUserSessionService, PersonalManagerAPI.Services.Implementation.UserSessionService>();

// Add RBAC (Role-Based Access Control) Services
builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.IRbacService, PersonalManagerAPI.Services.Implementation.RbacService>();

// Add Security Services for API Protection (temporarily disabled for compilation)
// builder.Services.AddScoped<PersonalManagerAPI.Services.Interfaces.ISecurityService, PersonalManagerAPI.Services.Implementation.SecurityService>();

// Add Background Services (temporarily disabled for compilation - missing ISecurityService dependency)
// builder.Services.AddHostedService<PersonalManagerAPI.Services.Background.SecurityCleanupService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
var issuer = jwtSettings["Issuer"] ?? "PersonalManagerAPI";
var audience = jwtSettings["Audience"] ?? "PersonalManagerClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero // 移除預設的5分鐘時鐘偏差
    };

    // 設定 JWT Bearer 事件處理
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError("JWT 認證失敗: {Error}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var username = context.Principal?.FindFirst("username")?.Value;
            logger.LogInformation("JWT 令牌驗證成功: {Username}", username);
            return Task.CompletedTask;
        }
    };
});

// Add Authorization
builder.Services.AddAuthorization();

// Configure CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add custom middleware (order is important)
app.UseErrorHandling(); // Must be first to catch all exceptions
app.UseRateLimiting(); // Rate limiting should be early in the pipeline
app.UseRequestLogging(); // Log requests after error handling and rate limiting

app.UseHttpsRedirection();

// Enable static files for file uploads
app.UseStaticFiles();

app.UseCors("AllowFrontend");

// Add Authentication and Authorization middleware
app.UseAuthentication(); // Must come before UseAuthorization
app.UseMiddleware<PersonalManagerAPI.Middleware.JwtTokenValidationMiddleware>(); // JWT Token blacklist validation
app.UseAuthorization();

app.MapControllers();

app.Run();
