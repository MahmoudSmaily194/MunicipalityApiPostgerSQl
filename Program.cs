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

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DBContext>(options =>
{
    // First try local connection from config
    var connStr = builder.Configuration.GetConnectionString("DBConnection");

    // Then try Railway environment variable
    var envConnStr = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrEmpty(envConnStr))
    {
        // Append SSL mode for Railway
        connStr = envConnStr + "?sslmode=Require";
    }

    options.UseNpgsql(connStr);
});




// ✅ Identity services for Guid-based User and Role
builder.Services.AddIdentity<User, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<DBContext>()
    .AddDefaultTokenProviders();

// ✅  Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INewsService, NewsService>();
builder.Services.AddScoped<IEventsService, EventsService>();
builder.Services.AddScoped<IMunicipalService, MunicipalService>();
builder.Services.AddScoped<IComplaintService, ComplaintService>();
builder.Services.AddTransient<ISendEmailService, SendEmailService>();
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

// Cores
var allowedOrigin = "https://localhost:5173"; // Vite dev server port

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Enables cookies and auth headers
    });
});


var app = builder.Build();
app.UseCors("FrontendPolicy"); // 🔁 Make sure it's applied before auth if needed

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.None, // or SameSiteMode.None if needed
    HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.SameAsRequest // Matches your Secure = false on HTTP
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCookiePolicy();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DBContext>();
    db.Database.Migrate();
}
app.Run();
