using System.Threading;
using System.Threading.Tasks;

namespace INFRASTRUCTURE_LAYER.Services
{
    /// <summary>
    /// Client for a local Ollama inference server. All AI text generation flows
    /// through this adapter so external callers stay agnostic to the model.
    /// </summary>
    public interface IOllamaService
    {
        /// <summary>
        /// Generate a free-form text reply from the local model.
        /// </summary>
        /// <param name="systemPrompt">System/role instructions for the model.</param>
        /// <param name="userPrompt">The user's request payload.</param>
        /// <returns>Raw text content; null if the model returned nothing usable.</returns>
        Task<string?> GenerateTextAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generate a structured JSON reply from the local model, deserialized as <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Target model for the JSON response.</typeparam>
        /// <param name="systemPrompt">System/role instructions for the model.</param>
        /// <param name="userPrompt">The user's request payload.</param>
        /// <returns>Deserialized object; null if the reply could not be parsed.</returns>
        Task<T?> GenerateJsonAsync<T>(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
    }
}