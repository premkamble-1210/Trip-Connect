using DOMAIN_LAYER.Repository;
using DOMAIN_LAYER.Entity.Trip;
using DOMAIN_LAYER.Enum;
using Microsoft.EntityFrameworkCore;  // ✅ Correct

namespace INFRASTRUCTURE_LAYER.Repository
{
    /// <summary>
    /// Trip Repository Implementation - Specific operations for Trip entity
    /// </summary>
    public class TripRepository : Repository<Trip>, ITripRepository
    {
        public TripRepository(DbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get trip by ID with TripDays eagerly loaded
        /// </summary>
        public override async Task<Trip> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(t => t.TripDays)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Get trips by status
        /// </summary>
        public async Task<IEnumerable<Trip>> GetTripsByStatusAsync(TripStatus status)
        {
            return await _dbSet.Where(t => t.Status == status).ToListAsync();
        }

        /// <summary>
        /// Get trips by location
        /// </summary>
        public async Task<IEnumerable<Trip>> GetTripsByLocationAsync(string location)
        {
            return await _dbSet.Where(t => t.Location.Contains(location)).ToListAsync();
        }

        /// <summary>
        /// Get trips by host/organizer
        /// </summary>
        public async Task<IEnumerable<Trip>> GetTripsByHostAsync(int hostId)
        {
            return await _dbSet.Where(t => t.HostId == hostId).ToListAsync();
        }

        /// <summary>
        /// Get upcoming trips (not started yet)
        /// </summary>
        public async Task<IEnumerable<Trip>> GetUpcomingTripsAsync()
        {
            return await _dbSet.Where(t => t.Status == TripStatus.Planned && t.StartDate > DateTime.Now)
                               .OrderBy(t => t.StartDate)
                               .ToListAsync();
        }

        /// <summary>
        /// Get trips within budget range
        /// </summary>
        public async Task<IEnumerable<Trip>> GetTripsByBudgetAsync(decimal minBudget, decimal maxBudget)
        {
            return await _dbSet.Where(t => t.Budget >= minBudget && t.Budget <= maxBudget).ToListAsync();
        }

        /// <summary>
        /// Get trips by travel type
        /// </summary>
        public async Task<IEnumerable<Trip>> GetTripsByTravelTypeAsync(string travelType)
        {
            return await _dbSet.Where(t => t.TravelType == travelType).ToListAsync();
        }

        /// <summary>
        /// Search trips by multiple criteria
        /// </summary>
        public async Task<IEnumerable<Trip>> SearchTripsAsync(string location, DateTime? startDate, decimal? maxBudget, string travelType)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(location))
                query = query.Where(t => t.Location.Contains(location));

            if (startDate.HasValue)
                query = query.Where(t => t.StartDate >= startDate.Value);

            if (maxBudget.HasValue)
                query = query.Where(t => t.Budget <= maxBudget.Value);

            if (!string.IsNullOrEmpty(travelType))
                query = query.Where(t => t.TravelType == travelType);

            return await query.ToListAsync();
        }

        /// <summary>
        /// Get trip with all related data (includes members, requests, expenses)
        /// </summary>
        public async Task<Trip> GetTripWithDetailsAsync(int tripId)
        {
            return await _dbSet.Include(t => t.TripMembers)
                               .Include(t => t.TripRequests)
                               .Include(t => t.Expenses)
                               .Include(t => t.Ratings)
                               .Include(t => t.TripDays)
                               .FirstOrDefaultAsync(t => t.Id == tripId);
        }
    }
}
