using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using PersonalManager.Api.Auth;
using PersonalManager.Api.Data;
using PersonalManager.Api.Middleware;
using PersonalManager.Api.Models;
using PersonalManager.Api.Repositories;
using PersonalManager.Api.Services;
using PersonalManager.Api.Settings;

var builder = WebApplication.CreateBuilder(args);

// JSON serialization
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
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

// FileStorage settings
builder.Services.Configure<FileStorageSettings>(builder.Configuration.GetSection("FileStorage"));

// File storage provider: S3-compatible (production) or local disk (dev)
var s3Config = builder.Configuration.GetSection("FileStorage:S3").Get<S3StorageSettings>();
if (s3Config?.IsConfigured == true)
{
    builder.Services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(
        new BasicAWSCredentials(s3Config.AccessKey, s3Config.SecretKey),
        new AmazonS3Config
        {
            ServiceURL = s3Config.ServiceUrl,
            ForcePathStyle = s3Config.ForcePathStyle,
        }
    ));
    builder.Services.AddScoped<IFileStorageProvider, S3FileStorageProvider>();
    Console.WriteLine("檔案儲存: S3-compatible Object Storage");
}
else
{
    builder.Services.AddScoped<IFileStorageProvider, LocalFileStorageProvider>();
    Console.WriteLine("檔案儲存: 本地磁碟（dev fallback）");
}

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

// Rate Limiting — .NET 7+ built-in
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Auth endpoints (login / register): 10 requests per minute per IP
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Public write endpoints (guestbook POST): 10 requests per minute per IP
    options.AddPolicy("public_write", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

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

// --- Database connectivity probe ---
// Try to detect server version (which opens a real connection).
// If this fails, fall back to local JSON files so development can continue.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
bool useDatabase = false;
ServerVersion? serverVersion = null;

if (!string.IsNullOrEmpty(connectionString))
{
    try
    {
        serverVersion = ServerVersion.AutoDetect(connectionString);
        useDatabase = true;
    }
    catch
    {
        // DB unreachable — will use JSON fallback
    }
}

if (useDatabase)
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(connectionString!, serverVersion!));

    // Repositories — EF Core (read/write to database)
    builder.Services.AddScoped<IRepository<User>, EfRepository<User>>();
    builder.Services.AddScoped<IRepository<PersonalProfile>, EfRepository<PersonalProfile>>();
    builder.Services.AddScoped<IRepository<Education>, EfRepository<Education>>();
    builder.Services.AddScoped<IRepository<WorkExperience>, EfRepository<WorkExperience>>();
    builder.Services.AddScoped<IRepository<Skill>, EfRepository<Skill>>();
    builder.Services.AddScoped<IRepository<Portfolio>, EfRepository<Portfolio>>();
    builder.Services.AddScoped<IRepository<CalendarEvent>, EfRepository<CalendarEvent>>();
    builder.Services.AddScoped<IRepository<TodoItem>, EfRepository<TodoItem>>();
    builder.Services.AddScoped<IRepository<WorkTask>, EfRepository<WorkTask>>();
    builder.Services.AddScoped<IRepository<BlogPost>, EfRepository<BlogPost>>();
    builder.Services.AddScoped<IRepository<GuestBookEntry>, EfRepository<GuestBookEntry>>();
    builder.Services.AddScoped<IRepository<ContactMethod>, EfRepository<ContactMethod>>();
    builder.Services.AddScoped<IRepository<FileUpload>, EfRepository<FileUpload>>();
    builder.Services.AddScoped<IRepository<PortfolioAttachment>, EfRepository<PortfolioAttachment>>();
    builder.Services.AddScoped<IRepository<TimeEntry>, EfRepository<TimeEntry>>();
    builder.Services.AddScoped<IRepository<RefreshToken>, EfRepository<RefreshToken>>();
}
else
{
    // Repositories — JSON fallback (reads/writes to Data/JsonData/*.json)
    builder.Services.AddScoped<IRepository<User>, JsonRepository<User>>();
    builder.Services.AddScoped<IRepository<PersonalProfile>, JsonRepository<PersonalProfile>>();
    builder.Services.AddScoped<IRepository<Education>, JsonRepository<Education>>();
    builder.Services.AddScoped<IRepository<WorkExperience>, JsonRepository<WorkExperience>>();
    builder.Services.AddScoped<IRepository<Skill>, JsonRepository<Skill>>();
    builder.Services.AddScoped<IRepository<Portfolio>, JsonRepository<Portfolio>>();
    builder.Services.AddScoped<IRepository<CalendarEvent>, JsonRepository<CalendarEvent>>();
    builder.Services.AddScoped<IRepository<TodoItem>, JsonRepository<TodoItem>>();
    builder.Services.AddScoped<IRepository<WorkTask>, JsonRepository<WorkTask>>();
    builder.Services.AddScoped<IRepository<BlogPost>, JsonRepository<BlogPost>>();
    builder.Services.AddScoped<IRepository<GuestBookEntry>, JsonRepository<GuestBookEntry>>();
    builder.Services.AddScoped<IRepository<ContactMethod>, JsonRepository<ContactMethod>>();
    builder.Services.AddScoped<IRepository<FileUpload>, JsonRepository<FileUpload>>();
    builder.Services.AddScoped<IRepository<PortfolioAttachment>, JsonRepository<PortfolioAttachment>>();
    builder.Services.AddScoped<IRepository<TimeEntry>, JsonRepository<TimeEntry>>();
    builder.Services.AddScoped<IRepository<RefreshToken>, JsonRepository<RefreshToken>>();
}

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
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IPortfolioAttachmentService, PortfolioAttachmentService>();
builder.Services.AddScoped<ITimeEntryService, TimeEntryService>();

// Health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is running"))
    .AddCheck<DbHealthCheck>("database");

var app = builder.Build();

// Startup: migrate/seed when DB is available; log mode when using JSON fallback
if (useDatabase)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    await DatabaseSeeder.CreateIndexesAsync(db);
    await DatabaseSeeder.SeedAsync(db);

    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("啟動模式: 資料庫 (MariaDB)");
}
else
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogWarning("啟動模式: JSON Fallback — 資料庫無法連線，使用本地 JSON 資料 (Data/JsonData/)");
}

// Middleware pipeline
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

// Static files for uploaded content
var fileStoragePath = Path.Combine(app.Environment.ContentRootPath,
    app.Configuration["FileStorage:RootPath"] ?? "files");
Directory.CreateDirectory(fileStoragePath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(fileStoragePath),
    RequestPath = "/files"
});

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/api/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        });
        await ctx.Response.WriteAsync(result);
    }
});

app.Run();
