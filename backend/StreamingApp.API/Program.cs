using System.Text;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Minio;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;
using StreamingApp.API.Hubs;
using StreamingApp.API.Middleware;
using StreamingApp.Application.Interfaces;
using StreamingApp.Application.Jobs;
using StreamingApp.Application.Services;
using StreamingApp.Domain.Entities;
using StreamingApp.Domain.Interfaces;
using StreamingApp.Infrastructure.Data;
using StreamingApp.Infrastructure.External;
using StreamingApp.Infrastructure.Repositories;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Seed check
var isSeedCommand = args.Contains("seed");

// DB
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<User, IdentityRole>(opt =>
{
    opt.Password.RequireDigit = true;
    opt.Password.RequiredLength = 8;
    opt.Password.RequireUppercase = true;
    opt.Password.RequireNonAlphanumeric = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// JWT
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
    opt.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                context.Token = accessToken;
            return Task.CompletedTask;
        }
    };
});

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"));

// MinIO
builder.Services.AddMinio(configureClient => configureClient
    .WithEndpoint(builder.Configuration["MinIO:Endpoint"] ?? "localhost:9000")
    .WithCredentials(
        builder.Configuration["MinIO:AccessKey"] ?? "minioadmin",
        builder.Configuration["MinIO:SecretKey"] ?? "minioadmin123")
    .WithSSL(builder.Configuration.GetValue<bool>("MinIO:UseSSL")));

// Hangfire
builder.Services.AddHangfire(config => config
    .UsePostgreSqlStorage(opt =>
        opt.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));
builder.Services.AddHangfireServer();

// Repositories
builder.Services.AddScoped<IContentRepository, ContentRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ContentService>();
builder.Services.AddScoped<WatchHistoryService>();
builder.Services.AddScoped<TranscodingJob>();

// External
builder.Services.AddScoped<IStorageService, MinioStorageService>();
builder.Services.AddScoped<IFfmpegService, FfmpegService>();

// SignalR
builder.Services.AddSignalR();
builder.Services.AddScoped<ITranscodingNotifier, SignalRTranscodingNotifier>();

// CORS
builder.Services.AddCors(opt => opt.AddDefaultPolicy(policy =>
    policy.WithOrigins("http://localhost:4200", "http://localhost", "http://localhost:80")
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter() }
});

app.MapControllers();
app.MapHub<TranscodingHub>("/hubs/transcoding");

// Seed or run
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await DatabaseSeeder.SeedAsync(context, userManager, roleManager);
}

if (!isSeedCommand) app.Run();
