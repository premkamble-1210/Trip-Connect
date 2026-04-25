using APPLICATION_LAYER;
using APPLICATION_LAYER.Models;
using APPLICATION_LAYER.Services.Interfaces;
using INFRASTRUCTURE_LAYER;
using API_LAYER.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Add JWT Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Add Token Service
builder.Services.AddScoped<ITokenService, APPLICATION_LAYER.Services.Implementations.TokenService>();
builder.Services.AddScoped<IRefreshTokenService, APPLICATION_LAYER.Services.Implementations.RefreshTokenService>();

// Add Authentication with JWT Bearer scheme
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
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // Custom JWT Bearer events for error handling
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            context.HandleResponse(); // prevents default 401 WWW-Authenticate challenge
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var isExpired = context.AuthenticateFailure is SecurityTokenExpiredException;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = isExpired ? "Token has expired" : "Authentication failed",
                errorCode = isExpired ? "TOKEN_EXPIRED" : "AUTH_FAILED"
            });
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsJsonAsync(new 
            { 
                success = false, 
                message = "Access denied. Insufficient permissions",
                errorCode = "FORBIDDEN"
            });
        }
    };
});

// Add Authorization
builder.Services.AddAuthorization();

// Add CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200",
            "https://localhost:4200",
            "http://localhost:3000",
            "https://localhost:3000"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

// Add Swagger
builder.Services.AddSwaggerGen();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("API", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is healthy"));

builder.Services.AddApplicationLayer(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TripConnect API v1");
        c.RoutePrefix = string.Empty;
    });
}

// Use Error Handling Middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

// Use CORS
app.UseCors("AllowAngularFrontend");

app.UseHttpsRedirection();

// Add Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map Health Check endpoints
app.MapHealthChecks("/api/health/check", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { status = report.Status.ToString(), checks = report.Entries });
    }
});

app.Run();
