using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using APPLICATION_LAYER.Mappers;
using APPLICATION_LAYER.Models;
using APPLICATION_LAYER.Services.Interfaces;
using APPLICATION_LAYER.Services.Implementations;
using Serilog;
using INFRASTRUCTURE_LAYER.Logging;

namespace APPLICATION_LAYER
{
    /// <summary>
    /// Extension methods for registering Application Layer services
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Register Application Layer services including AutoMapper profiles
        /// </summary>
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure and register Serilog
            var logger = LoggingConfiguration.ConfigureLogger();
            Log.Logger = logger;
            services.AddSingleton<Serilog.ILogger>(logger);

            // Bind EmailSettings from configuration
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            // Register Email Service
            services.AddScoped<IEmailService, EmailService>();

            // Register Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITripService, TripService>();
            services.AddScoped<IJoinRequestService, JoinRequestService>();
            services.AddScoped<IExpenseService, ExpenseService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IRatingService, RatingService>();

            // Add AutoMapper
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<UserProfile>();
                cfg.AddProfile<TripProfile>();
                cfg.AddProfile<TripRequestProfile>();
                cfg.AddProfile<ExpenseProfile>();
                cfg.AddProfile<ChatMessageProfile>();
                cfg.AddProfile<TripRatingProfile>();
            });

            return services;
        }
    }
}
