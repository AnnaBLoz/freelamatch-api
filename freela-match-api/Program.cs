using FreelaMatchAPI.Data;
using FreelaMatchAPI.Interfaces;
using FreelaMatchAPI.Models;
using FreelaMatchAPI.Services;
using FreelaMatchAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------
// OPEN TELEMETRY NEW RELIC
// ----------------------------
builder.Services.AddOpenTelemetry()
    .ConfigureResource(res =>
        res.AddService("FreelaMatchAPI"))
    .WithTracing(tracer =>
    {
        tracer.AddAspNetCoreInstrumentation()
              .AddHttpClientInstrumentation()
              .AddEntityFrameworkCoreInstrumentation()
              .AddOtlpExporter(o =>
              {
                  o.Endpoint = new Uri("https://otlp.nr-data.net:4317");
                  o.Headers = "api-key=SUA_LICENSE_KEY_AQUI";
              });
    })
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
               .AddRuntimeInstrumentation()
               .AddHttpClientInstrumentation()
               .AddOtlpExporter(o =>
               {
                   o.Endpoint = new Uri("https://otlp.nr-data.net:4317");
                   o.Headers = "api-key=SUA_LICENSE_KEY_AQUI";
               });
    });

// ----------------------------
// CORS
// ----------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// ----------------------------
// DATABASE (MySQL)
// ----------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 33))
    )
);

// ----------------------------
// SERVICES
// ----------------------------
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<PortfolioService>();
builder.Services.AddScoped<GeneralService>();
builder.Services.AddScoped<ProposalService>();
builder.Services.AddScoped<ReviewsService>();
builder.Services.AddScoped<EmailService>();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGeneralService, GeneralService>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IProposalService, ProposalService>();
builder.Services.AddScoped<IReviewsService, ReviewsService>();
builder.Services.AddScoped<IUserService, UserService>();

// ----------------------------
// CONTROLLERS + SWAGGER
// ----------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ----------------------------
// JWT CONFIGURATION
// ----------------------------
var jwtSettings = builder.Configuration.GetSection("Jwt");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Key"]!)
        )
    };
});

// ----------------------------
// CUSTOM URLS
// ----------------------------
builder.WebHost.UseUrls("https://localhost:5000", "http://localhost:5001");

var app = builder.Build();

// ----------------------------
// MIDDLEWARE
// ----------------------------

app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();