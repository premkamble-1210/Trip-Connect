using DOMAIN_LAYER.Entity.User;
using DOMAIN_LAYER.Entity.Trip;
using DOMAIN_LAYER.Entity.TripRequest;
using DOMAIN_LAYER.Entity.TripMember;
using DOMAIN_LAYER.Entity.Expense;
using DOMAIN_LAYER.Entity.ExpenseSplit;
using DOMAIN_LAYER.Entity.ChatMessage;
using DOMAIN_LAYER.Entity.TripRating;
using DOMAIN_LAYER.Entity.Cache;
using Microsoft.EntityFrameworkCore;

namespace INFRASTRUCTURE_LAYER.Data
{
    /// <summary>
    /// TripConnect Application DbContext - Manages database connections and entity mappings
    /// </summary>
    public class TripConnectDbContext : DbContext
    {
        /// <summary>
        /// Constructor - Initializes DbContext with options
        /// </summary>
        public TripConnectDbContext(DbContextOptions<TripConnectDbContext> options) : base(options)
        {
        }

        #region DbSets

        /// <summary>
        /// Users table
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Trips table
        /// </summary>
        public DbSet<Trip> Trips { get; set; }

        /// <summary>
        /// Trip Requests table
        /// </summary>
        public DbSet<TripRequest> TripRequests { get; set; }

        /// <summary>
        /// Trip Members table
        /// </summary>
        public DbSet<TripMember> TripMembers { get; set; }

        /// <summary>
        /// Expenses table
        /// </summary>
        public DbSet<Expense> Expenses { get; set; }

        /// <summary>
        /// Expense Splits table
        /// </summary>
        public DbSet<ExpenseSplit> ExpenseSplits { get; set; }

        /// <summary>
        /// Chat Messages table
        /// </summary>
        public DbSet<ChatMessage> ChatMessages { get; set; }

        /// <summary>
        /// Trip Ratings table
        /// </summary>
        public DbSet<TripRating> TripRatings { get; set; }

        /// <summary>
        /// Trip Days (Itinerary) table
        /// </summary>
        public DbSet<TripDay> TripDays { get; set; }

        /// <summary>
        /// Cache Policies table
        /// </summary>
        public DbSet<CachePolicy> CachePolicies { get; set; }

        /// <summary>
        /// Cache Entries table
        /// </summary>
        public DbSet<CacheEntry> CacheEntries { get; set; }

        #endregion

        /// <summary>
        /// Configure entity relationships and constraints
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasMany(u => u.CreatedTrips)
                .WithOne(t => t.Host)
                .HasForeignKey(t => t.HostId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.TripRequests)
                .WithOne(tr => tr.User)
                .HasForeignKey(tr => tr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.TripMembers)
                .WithOne(tm => tm.User)
                .HasForeignKey(tm => tm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.PaidExpenses)
                .WithOne(e => e.PaidByUser)
                .HasForeignKey(e => e.PaidBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.ExpenseSplits)
                .WithOne(es => es.User)
                .HasForeignKey(es => es.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.ChatMessages)
                .WithOne(cm => cm.Sender)
                .HasForeignKey(cm => cm.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.RatingsGiven)
                .WithOne(r => r.RatedByUser)
                .HasForeignKey(r => r.RatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.RatingsReceived)
                .WithOne(r => r.RatedUser)
                .HasForeignKey(r => r.RatedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Trip entity
            modelBuilder.Entity<Trip>()
                .HasKey(t => t.Id);

            modelBuilder.Entity<Trip>()
                .HasMany(t => t.TripRequests)
                .WithOne(tr => tr.Trip)
                .HasForeignKey(tr => tr.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Trip>()
                .HasMany(t => t.TripMembers)
                .WithOne(tm => tm.Trip)
                .HasForeignKey(tm => tm.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Trip>()
                .HasMany(t => t.Expenses)
                .WithOne(e => e.Trip)
                .HasForeignKey(e => e.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Trip>()
                .HasMany(t => t.ChatMessages)
                .WithOne(cm => cm.Trip)
                .HasForeignKey(cm => cm.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Trip>()
                .HasMany(t => t.Ratings)
                .WithOne(r => r.Trip)
                .HasForeignKey(r => r.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Trip>()
                .HasMany(t => t.TripDays)
                .WithOne(td => td.Trip)
                .HasForeignKey(td => td.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure TripRequest entity
            modelBuilder.Entity<TripRequest>()
                .HasKey(tr => tr.Id);

            modelBuilder.Entity<TripRequest>()
                .HasIndex(tr => new { tr.TripId, tr.UserId })
                .IsUnique();

            // Configure TripMember entity
            modelBuilder.Entity<TripMember>()
                .HasKey(tm => tm.Id);

            modelBuilder.Entity<TripMember>()
                .HasIndex(tm => new { tm.TripId, tm.UserId })
                .IsUnique();

            // Configure Expense entity
            modelBuilder.Entity<Expense>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<Expense>()
                .HasMany(e => e.ExpenseSplits)
                .WithOne(es => es.Expense)
                .HasForeignKey(es => es.ExpenseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ExpenseSplit entity
            modelBuilder.Entity<ExpenseSplit>()
                .HasKey(es => es.Id);

            // Configure ChatMessage entity
            modelBuilder.Entity<ChatMessage>()
                .HasKey(cm => cm.Id);

            // Configure TripRating entity
            modelBuilder.Entity<TripRating>()
                .HasKey(r => r.Id);

            modelBuilder.Entity<TripRating>()
                .HasIndex(r => new { r.TripId, r.RatedBy, r.RatedUserId })
                .IsUnique();

            // Configure CachePolicy entity
            modelBuilder.Entity<CachePolicy>()
                .HasKey(cp => cp.Id);

            modelBuilder.Entity<CachePolicy>()
                .HasIndex(cp => cp.PolicyName)
                .IsUnique();

            modelBuilder.Entity<CachePolicy>()
                .Property(cp => cp.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Configure CacheEntry entity
            modelBuilder.Entity<CacheEntry>()
                .HasKey(ce => ce.Id);

            modelBuilder.Entity<CacheEntry>()
                .HasIndex(ce => ce.CacheKey)
                .IsUnique();

            modelBuilder.Entity<CacheEntry>()
                .HasIndex(ce => ce.ExpiresAt);

            modelBuilder.Entity<CacheEntry>()
                .HasIndex(ce => ce.DataType);

            modelBuilder.Entity<CacheEntry>()
                .Property(ce => ce.CachedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<CacheEntry>()
                .HasOne(ce => ce.CachePolicy)
                .WithMany()
                .HasForeignKey(ce => ce.CachePolicyId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
