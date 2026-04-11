using DOMAIN_LAYER.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace INFRASTRUCTURE_LAYER.Repository
{
    /// <summary>
    /// Unit of Work Implementation - Manages all repositories and transaction coordination
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _context;
        private IDbContextTransaction _transaction;

        private IUserRepository _userRepository;
        private ITripRepository _tripRepository;
        private ITripRequestRepository _tripRequestRepository;
        private ITripMemberRepository _tripMemberRepository;
        private IExpenseRepository _expenseRepository;
        private IExpenseSplitRepository _expenseSplitRepository;
        private IChatRepository _chatRepository;
        private ITripRatingRepository _tripRatingRepository;

        /// <summary>
        /// Constructor - Initializes UnitOfWork with database context
        /// </summary>
        public UnitOfWork(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// User Repository
        /// </summary>
        public IUserRepository Users
        {
            get
            {
                _userRepository ??= new UserRepository(_context);
                return _userRepository;
            }
        }

        /// <summary>
        /// Trip Repository
        /// </summary>
        public ITripRepository Trips
        {
            get
            {
                _tripRepository ??= new TripRepository(_context);
                return _tripRepository;
            }
        }

        /// <summary>
        /// Trip Request Repository
        /// </summary>
        public ITripRequestRepository TripRequests
        {
            get
            {
                _tripRequestRepository ??= new TripRequestRepository(_context);
                return _tripRequestRepository;
            }
        }

        /// <summary>
        /// Trip Member Repository
        /// </summary>
        public ITripMemberRepository TripMembers
        {
            get
            {
                _tripMemberRepository ??= new TripMemberRepository(_context);
                return _tripMemberRepository;
            }
        }

        /// <summary>
        /// Expense Repository
        /// </summary>
        public IExpenseRepository Expenses
        {
            get
            {
                _expenseRepository ??= new ExpenseRepository(_context);
                return _expenseRepository;
            }
        }

        /// <summary>
        /// Expense Split Repository
        /// </summary>
        public IExpenseSplitRepository ExpenseSplits
        {
            get
            {
                _expenseSplitRepository ??= new ExpenseSplitRepository(_context);
                return _expenseSplitRepository;
            }
        }

        /// <summary>
        /// Chat Message Repository
        /// </summary>
        public IChatRepository ChatMessages
        {
            get
            {
                _chatRepository ??= new ChatRepository(_context);
                return _chatRepository;
            }
        }

        /// <summary>
        /// Trip Rating Repository
        /// </summary>
        public ITripRatingRepository TripRatings
        {
            get
            {
                _tripRatingRepository ??= new TripRatingRepository(_context);
                return _tripRatingRepository;
            }
        }

        /// <summary>
        /// Check if there is an active transaction
        /// </summary>
        public bool HasActiveTransaction => _transaction != null;

        /// <summary>
        /// Save all changes to the database
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Begin a database transaction
        /// </summary>
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// Commit the current transaction
        /// </summary>
        public async Task CommitTransactionAsync()
        {
            try
            {
                await SaveChangesAsync();
                await _transaction?.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }

        /// <summary>
        /// Rollback the current transaction
        /// </summary>
        public async Task RollbackTransactionAsync()
        {
            try
            {
                await _transaction?.RollbackAsync();
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            _transaction?.Dispose();
            _context?.Dispose();
        }
    }
}
