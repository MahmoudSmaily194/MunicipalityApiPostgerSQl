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
        var databaseUrl = new Uri(envConnStr);
        var userInfo = databaseUrl.UserInfo.Split(':', 2);
        var username = userInfo[0];
        var password = userInfo.Length > 1 ? userInfo[1] : "";
        var host = databaseUrl.Host;
        var port = databaseUrl.Port;
        var db = databaseUrl.AbsolutePath.Trim('/');

        connStr = $"Host={host};Port={port};Database={db};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    }

    // ✅ Enable retry on failure & set command timeout
    options.UseNpgsql(connStr, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null
        );
        npgsqlOptions.CommandTimeout(60); // 60 seconds
    });
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
builder.Services.AddHttpClient("SupabaseStorageClient");
builder.Services.AddScoped<SawirahMunicipalityWeb.Services.ImageService.SupabaseImageService>();

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
var allowedOrigins = new[]
{
    "https://localhost:5173",
    "https://sawirah.netlify.app"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ------------------- Middleware -------------------
app.UseCors("FrontendPolicy");
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

// ------------------- Safe Database Migration with Retry -------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DBContext>();
    int retries = 10;
    while (retries > 0)
    {
        try
        {
            db.Database.Migrate();
            Console.WriteLine("Database migrated successfully.");
            break;
        }
        catch (Exception ex)
        {
            retries--;
            Console.WriteLine($"Migration failed: {ex.Message}. Retrying in 5 seconds... ({retries} retries left)");
            Thread.Sleep(5000);
        }
    }

    if (retries == 0)
        Console.WriteLine("Database migration failed after multiple attempts. App will continue.");

    // Log all endpoints
    var endpoints = app.Services.GetRequiredService<EndpointDataSource>()
        .Endpoints.OfType<RouteEndpoint>();
    foreach (var endpoint in endpoints)
    {
        var methods = string.Join(",", endpoint.Metadata
            .OfType<HttpMethodMetadata>()
            .FirstOrDefault()?.HttpMethods ?? new List<string>());
        Console.WriteLine($"{endpoint.RoutePattern.RawText} [{methods}]");
    }
}

// ------------------- Listen on Railway PORT -------------------
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();
