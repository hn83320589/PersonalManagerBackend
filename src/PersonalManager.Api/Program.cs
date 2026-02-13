using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PersonalManager.Api.Auth;
using PersonalManager.Api.Middleware;
using PersonalManager.Api.Models;
using PersonalManager.Api.Repositories;
using PersonalManager.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// JSON serialization
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.PropertyNamingPolicy = null; // PascalCase to match DTOs
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Personal Manager API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:4173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Repositories (JsonRepository for all entities)
builder.Services.AddSingleton<IRepository<User>, JsonRepository<User>>();
builder.Services.AddSingleton<IRepository<PersonalProfile>, JsonRepository<PersonalProfile>>();
builder.Services.AddSingleton<IRepository<Education>, JsonRepository<Education>>();
builder.Services.AddSingleton<IRepository<WorkExperience>, JsonRepository<WorkExperience>>();
builder.Services.AddSingleton<IRepository<Skill>, JsonRepository<Skill>>();
builder.Services.AddSingleton<IRepository<Portfolio>, JsonRepository<Portfolio>>();
builder.Services.AddSingleton<IRepository<CalendarEvent>, JsonRepository<CalendarEvent>>();
builder.Services.AddSingleton<IRepository<TodoItem>, JsonRepository<TodoItem>>();
builder.Services.AddSingleton<IRepository<WorkTask>, JsonRepository<WorkTask>>();
builder.Services.AddSingleton<IRepository<BlogPost>, JsonRepository<BlogPost>>();
builder.Services.AddSingleton<IRepository<GuestBookEntry>, JsonRepository<GuestBookEntry>>();
builder.Services.AddSingleton<IRepository<ContactMethod>, JsonRepository<ContactMethod>>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IEducationService, EducationService>();
builder.Services.AddScoped<IWorkExperienceService, WorkExperienceService>();
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<ICalendarEventService, CalendarEventService>();
builder.Services.AddScoped<ITodoItemService, TodoItemService>();
builder.Services.AddScoped<IWorkTaskService, WorkTaskService>();
builder.Services.AddScoped<IBlogPostService, BlogPostService>();
builder.Services.AddScoped<IGuestBookEntryService, GuestBookEntryService>();
builder.Services.AddScoped<IContactMethodService, ContactMethodService>();

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
