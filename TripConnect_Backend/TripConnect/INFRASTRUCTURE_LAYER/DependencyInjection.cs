using DOMAIN_LAYER.Repository;
using INFRASTRUCTURE_LAYER.Data;
using INFRASTRUCTURE_LAYER.Repository;
using INFRASTRUCTURE_LAYER.Repository.Cache;
using INFRASTRUCTURE_LAYER.Logging;
using INFRASTRUCTURE_LAYER.Cache;
using INFRASTRUCTURE_LAYER.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Binder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
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
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure()));

            // Register DbContext as generic DbContext for repositories
            services.AddScoped<DbContext>(provider => provider.GetRequiredService<TripConnectDbContext>());

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

            // Configure and Register Redis Cache Services
            AddRedisCacheServices(services, configuration);

            // Register HttpClient factory and ImageKit Service
            services.AddSingleton<HttpClient>();
            services.AddScoped<IImageKitService, ImageKitService>();

            // Register AI adapters (Ollama + pollinations.ai banner)
            services.AddScoped<IOllamaService, OllamaService>();
            services.AddScoped<IBannerService, BannerService>();

            return services;
        }

        /// <summary>
        /// Configure Redis cache services
        /// </summary>
        private static void AddRedisCacheServices(
            IServiceCollection services,
            IConfiguration configuration)
        {
            // Configure Redis settings
            var redisCacheConfig = new RedisCacheConfiguration();
            //configuration.GetSection("RedisCache");
            configuration.GetSection("RedisCache").Bind(redisCacheConfig);
            // Validate Redis configuration
            if (string.IsNullOrEmpty(redisCacheConfig.ConnectionString))
            {
                throw new InvalidOperationException(
                    "Redis connection string is not configured. " +
                    "Please configure 'RedisCache:ConnectionString' in appsettings.json");
            }

            // Register configuration as singleton
            services.AddSingleton(redisCacheConfig);

            // Register Redis connection multiplexer as singleton
            services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                var config = provider.GetRequiredService<RedisCacheConfiguration>();
                return RedisConnectionFactory.GetConnection(config);
            });

            // Register Cache Repositories
            services.AddScoped<ICacheRepository, CacheRepository>();
            services.AddScoped<ICachePolicyRepository, CachePolicyRepository>();
            services.AddScoped<ICacheEntryRepository, CacheEntryRepository>();
            services.AddScoped<ICacheInvalidationRepository, CacheInvalidationRepository>();

            // Register Cache Service
            services.AddScoped<ICacheService, CacheService>();

            // Register Cache Statistics Service
            services.AddScoped<ICacheStatisticsService, CacheStatisticsService>();

            // Register Cache Warming Service
            services.AddScoped<ICacheWarmingService, CacheWarmingService>();

            // Register Background Cache Cleanup Service
            services.AddSingleton<CacheCleanupBackgroundService>();
            services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<CacheCleanupBackgroundService>());
        }
    }
}
