namespace DOMAIN_LAYER.Repository
{
    /// <summary>
    /// Generic Repository Interface - Provides common CRUD operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Get entity by ID
        /// </summary>
        Task<T> GetByIdAsync(int id);

        /// <summary>
        /// Get all entities
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Add new entity
        /// </summary>
        Task AddAsync(T entity);

        /// <summary>
        /// Update existing entity
        /// </summary>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Delete entity
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Save changes to database
        /// </summary>
        Task SaveChangesAsync();

        /// <summary>
        /// Check if entity exists
        /// </summary>
        Task<bool> ExistsAsync(int id);
    }
}
