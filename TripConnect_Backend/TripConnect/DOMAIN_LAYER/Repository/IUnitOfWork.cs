namespace DOMAIN_LAYER.Repository
{
    /// <summary>
    /// Unit of Work Interface - Manages all repositories and transaction coordination
    /// Implements the Unit of Work pattern for repository management
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// User Repository
        /// </summary>
        IUserRepository Users { get; }

        /// <summary>
        /// Trip Repository
        /// </summary>
        ITripRepository Trips { get; }

        /// <summary>
        /// Trip Request Repository
        /// </summary>
        ITripRequestRepository TripRequests { get; }

        /// <summary>
        /// Trip Member Repository
        /// </summary>
        ITripMemberRepository TripMembers { get; }

        /// <summary>
        /// Expense Repository
        /// </summary>
        IExpenseRepository Expenses { get; }

        /// <summary>
        /// Expense Split Repository
        /// </summary>
        IExpenseSplitRepository ExpenseSplits { get; }

        /// <summary>
        /// Chat Message Repository
        /// </summary>
        IChatRepository ChatMessages { get; }

        /// <summary>
        /// Trip Rating Repository
        /// </summary>
        ITripRatingRepository TripRatings { get; }

        /// <summary>
        /// Save all changes to the database
        /// </summary>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Begin a database transaction
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// Commit the current transaction
        /// </summary>
        Task CommitTransactionAsync();

        /// <summary>
        /// Rollback the current transaction
        /// </summary>
        Task RollbackTransactionAsync();

        /// <summary>
        /// Check if there is an active transaction
        /// </summary>
        bool HasActiveTransaction { get; }
    }
}
