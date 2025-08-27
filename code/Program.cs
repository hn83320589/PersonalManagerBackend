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

// Configure Entity Framework (commented out for JSON data service)
// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Add JSON Data Service for development
builder.Services.AddScoped<JsonDataService>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(PersonalManagerAPI.Mappings.MappingProfile));

// Add Business Service Layer (starting with User service)
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

// Add File Services for media handling
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IFileSecurityService, FileSecurityService>();
builder.Services.AddScoped<IFileQuarantineService, FileQuarantineService>();

// Add Authentication Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<PersonalManagerAPI.Services.Interfaces.ITokenBlacklistService, PersonalManagerAPI.Services.Implementation.TokenBlacklistService>();

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
app.UseRequestLogging(); // Log requests after error handling

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
