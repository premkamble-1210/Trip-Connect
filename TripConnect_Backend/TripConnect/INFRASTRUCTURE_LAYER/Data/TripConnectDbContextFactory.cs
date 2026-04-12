using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace INFRASTRUCTURE_LAYER.Data
{
    /// <summary>
    /// Design-time DbContext factory for Entity Framework Core migrations
    /// </summary>
    public class TripConnectDbContextFactory : IDesignTimeDbContextFactory<TripConnectDbContext>
    {
        /// <summary>
        /// Create DbContext instance for migration operations
        /// </summary>
        public TripConnectDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TripConnectDbContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=TripConnect;Trusted_Connection=True;MultipleActiveResultSets=true");
            return new TripConnectDbContext(optionsBuilder.Options);
        }
    }
}
