using APPLICATION_LAYER;
using INFRASTRUCTURE_LAYER;
using API_LAYER.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

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

builder.Services.AddApplicationLayer();
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
