using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace INFRASTRUCTURE_LAYER.Logging
{
    /// <summary>
    /// Serilog Configuration - Minimal logging setup for file logging
    /// </summary>
    public static class LoggingConfiguration
    {
        /// <summary>
        /// Configure Serilog logger with minimal important logs only
        /// </summary>
        public static Logger ConfigureLogger()
        {
            return new LoggerConfiguration()
                // Minimum level - only log Important events (Errors, Warnings, and Information for critical operations)
                .MinimumLevel.Information()
                
                // Override to ignore verbose logs
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                
                // Write to file
                .WriteTo.File(
                    path: "logs/tripconnect-.txt",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    retainedFileCountLimit: 7)
                
                // Create logger
                .CreateLogger();
        }

        /// <summary>
        /// Get logger for specific type
        /// </summary>
        public static ILogger GetLogger<T>()
        {
            return Log.ForContext<T>();
        }

        /// <summary>
        /// Log important information (e.g., user actions, trip creation)
        /// </summary>
        public static void LogInfo(string message, params object[] args)
        {
            Log.Information(message, args);
        }

        /// <summary>
        /// Log warnings (e.g., validation failures, deprecated operations)
        /// </summary>
        public static void LogWarning(string message, params object[] args)
        {
            Log.Warning(message, args);
        }

        /// <summary>
        /// Log errors (e.g., database failures, service errors)
        /// </summary>
        public static void LogError(Exception ex, string message, params object[] args)
        {
            Log.Error(ex, message, args);
        }

        /// <summary>
        /// Log critical errors (e.g., system failures)
        /// </summary>
        public static void LogCritical(Exception ex, string message, params object[] args)
        {
            Log.Fatal(ex, message, args);
        }
    }
}
