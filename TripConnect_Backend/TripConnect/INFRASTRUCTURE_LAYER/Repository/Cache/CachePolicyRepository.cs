using DOMAIN_LAYER.Repository;
using DOMAIN_LAYER.Entity.Cache;
using Microsoft.EntityFrameworkCore;

namespace INFRASTRUCTURE_LAYER.Repository.Cache
{
    /// <summary>
    /// Cache Policy Repository Implementation - Manages cache policies in database
    /// </summary>
    public class CachePolicyRepository : Repository<CachePolicy>, ICachePolicyRepository
    {
        /// <summary>
        /// Constructor - Initializes with database context
        /// </summary>
        public CachePolicyRepository(DbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get cache policy by name
        /// </summary>
        public async Task<CachePolicy> GetByPolicyNameAsync(string policyName)
        {
            if (string.IsNullOrEmpty(policyName))
                throw new ArgumentNullException(nameof(policyName));

            return await _dbSet
                .FirstOrDefaultAsync(cp => cp.PolicyName == policyName && cp.IsActive);
        }

        /// <summary>
        /// Get all active cache policies
        /// </summary>
        public async Task<IEnumerable<CachePolicy>> GetActivePoliciesAsync()
        {
            return await _dbSet
                .Where(cp => cp.IsActive)
                .OrderBy(cp => cp.PolicyName)
                .ToListAsync();
        }

        /// <summary>
        /// Get cache policy by type
        /// </summary>
        public async Task<IEnumerable<CachePolicy>> GetByPolicyTypeAsync(DOMAIN_LAYER.Enum.CachePolicyType policyType)
        {
            return await _dbSet
                .Where(cp => cp.PolicyType == policyType && cp.IsActive)
                .OrderBy(cp => cp.PolicyName)
                .ToListAsync();
        }

        /// <summary>
        /// Disable a cache policy
        /// </summary>
        public async Task<bool> DisablePolicyAsync(int policyId)
        {
            var policy = await GetByIdAsync(policyId);
            if (policy == null)
                return false;

            policy.IsActive = false;
            policy.ModifiedAt = DateTime.UtcNow;
            await UpdateAsync(policy);
            await SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Enable a cache policy
        /// </summary>
        public async Task<bool> EnablePolicyAsync(int policyId)
        {
            var policy = await GetByIdAsync(policyId);
            if (policy == null)
                return false;

            policy.IsActive = true;
            policy.ModifiedAt = DateTime.UtcNow;
            await UpdateAsync(policy);
            await SaveChangesAsync();

            return true;
        }
    }
}
