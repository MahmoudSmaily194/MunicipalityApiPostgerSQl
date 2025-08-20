using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SawirahMunicipalityWeb.Data;
using SawirahMunicipalityWeb.Entities;
using SawirahMunicipalityWeb.Services.AuhtServices;
using SawirahMunicipalityWeb.Services.ComplaintServices;
using SawirahMunicipalityWeb.Services.EventsServices;
using SawirahMunicipalityWeb.Services.MunicipalServices;
using SawirahMunicipalityWeb.Services.NewsServices;
using SawirahMunicipalityWeb.Services.SendEmailServices;
using System;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ------------------- Controllers & Swagger -------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// ------------------- Database -------------------
builder.Services.AddDbContext<DBContext>(options =>
{
    var connStr = builder.Configuration.GetConnectionString("DBConnection");

    var envConnStr = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrEmpty(envConnStr))
    {
        // Parse DATABASE_URL from Railway: postgresql://user:pass@host:port/dbname
        var databaseUrl = new Uri(envConnStr);

        var userInfo = databaseUrl.UserInfo.Split(':', 2);
        var username = userInfo[0];
        var password = userInfo.Length > 1 ? userInfo[1] : "";
        var host = databaseUrl.Host;
        var port = databaseUrl.Port;
        var db = databaseUrl.AbsolutePath.Trim('/');

        connStr =
            $"Host={host};Port={port};Database={db};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    }

    options.UseNpgsql(connStr);
});

// ------------------- Identity -------------------
builder.Services.AddIdentity<User, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<DBContext>()
    .AddDefaultTokenProviders();

// ------------------- Services -------------------
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INewsService, NewsService>();
builder.Services.AddScoped<IEventsService, EventsService>();
builder.Services.AddScoped<IMunicipalService, MunicipalService>();
builder.Services.AddScoped<IComplaintService, ComplaintService>();
builder.Services.AddTransient<ISendEmailService, SendEmailService>();

// ------------------- JWT Authentication -------------------
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
        ValidIssuer = builder.Configuration["AppSettings:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["AppSettings:Audience"],
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!)),
        ValidateIssuerSigningKey = true,
        RoleClaimType = ClaimTypes.Role,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddHttpContextAccessor();

// ------------------- CORS -------------------
var allowedOrigin = builder.Configuration["AllowedOrigin"] ?? "https://localhost:5173";

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("FrontendPolicy");

// ------------------- Middleware -------------------
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.None,
    HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.SameAsRequest
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Municipality API V1");
});

app.UseHealthChecks("/health");
//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();

// ------------------- Database Migration -------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DBContext>();
    //db.Database.Migrate();
}

// ------------------- Listen on Railway PORT -------------------
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();
