using DOMAIN_LAYER.Repository;
using INFRASTRUCTURE_LAYER.Data;
using INFRASTRUCTURE_LAYER.Repository;
using INFRASTRUCTURE_LAYER.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace INFRASTRUCTURE_LAYER
{
    /// <summary>
    /// Dependency Injection Extension - Registers Infrastructure Layer services
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Add Infrastructure services to the dependency injection container
        /// </summary>
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configure Serilog
            Log.Logger = LoggingConfiguration.ConfigureLogger();

            // Add DbContext
            services.AddDbContext<TripConnectDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection")));

            // Register Generic Repository
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Register Specific Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITripRepository, TripRepository>();
            services.AddScoped<ITripRequestRepository, TripRequestRepository>();
            services.AddScoped<ITripMemberRepository, TripMemberRepository>();
            services.AddScoped<IExpenseRepository, ExpenseRepository>();
            services.AddScoped<IExpenseSplitRepository, ExpenseSplitRepository>();
            services.AddScoped<IChatRepository, ChatRepository>();
            services.AddScoped<ITripRatingRepository, TripRatingRepository>();

            // Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
