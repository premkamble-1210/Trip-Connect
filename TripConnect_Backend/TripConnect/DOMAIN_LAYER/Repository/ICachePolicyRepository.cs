namespace DOMAIN_LAYER.Repository
{
    /// <summary>
    /// Cache Policy Repository Interface - For managing cache policies
    /// </summary>
    public interface ICachePolicyRepository : IRepository<Entity.Cache.CachePolicy>
    {
        /// <summary>
        /// Get cache policy by name
        /// </summary>
        Task<Entity.Cache.CachePolicy> GetByPolicyNameAsync(string policyName);

        /// <summary>
        /// Get all active cache policies
        /// </summary>
        Task<IEnumerable<Entity.Cache.CachePolicy>> GetActivePoliciesAsync();

        /// <summary>
        /// Get cache policy by type
        /// </summary>
        Task<IEnumerable<Entity.Cache.CachePolicy>> GetByPolicyTypeAsync(Enum.CachePolicyType policyType);

        /// <summary>
        /// Disable a cache policy
        /// </summary>
        Task<bool> DisablePolicyAsync(int policyId);

        /// <summary>
        /// Enable a cache policy
        /// </summary>
        Task<bool> EnablePolicyAsync(int policyId);
    }
}
