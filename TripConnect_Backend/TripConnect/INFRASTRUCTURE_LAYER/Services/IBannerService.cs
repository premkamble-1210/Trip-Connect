using System.Threading;
using System.Threading.Tasks;

namespace INFRASTRUCTURE_LAYER.Services
{
    /// <summary>
    /// Generates travel banner images through the free pollinations.ai image API.
    /// </summary>
    public interface IBannerService
    {
        /// <summary>
        /// Build the pollinations.ai image URL for a given visual prompt.
        /// </summary>
        string BuildBannerUrl(string prompt);

        /// <summary>
        /// Download the generated banner image bytes for a given visual prompt.
        /// </summary>
        /// <param name="prompt">Visual prompt describing the banner.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Image bytes, or null if generation/download failed.</returns>
        Task<byte[]?> DownloadBannerImageAsync(string prompt, CancellationToken cancellationToken = default);
    }
}