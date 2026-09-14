using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace INFRASTRUCTURE_LAYER.Services
{
    /// <summary>
    /// Implementation of <see cref="IOllamaService"/> that talks to the Ollama
    /// inference endpoint (POST /api/chat) over HTTP. Configured via the
    /// "Ollama" configuration section. Fails open with a logged warning when the
    /// local server is unreachable so the rest of the app keeps working.
    /// </summary>
    public class OllamaService : IOllamaService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        private const string CHAT_ENDPOINT = "/api/chat";

        private readonly ILogger<OllamaService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _model;
        private readonly TimeSpan _timeout;

        public OllamaService(IConfiguration configuration, ILogger<OllamaService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;

            _baseUrl = (configuration["Ollama:BaseUrl"] ?? "http://localhost:11434").TrimEnd('/');
            _model = configuration["Ollama:Model"] ?? "llama3.2";

            var timeoutSeconds = int.TryParse(configuration["Ollama:TimeoutSeconds"], out var timeout)
                ? timeout
                : 180;
            _timeout = TimeSpan.FromSeconds(timeoutSeconds);

            _logger.LogInformation("Ollama service configured. BaseUrl: {BaseUrl}, Model: {Model}, Timeout: {Timeout}s",
                _baseUrl, _model, timeoutSeconds);
        }

        public async Task<string?> GenerateTextAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
            => await SendChatAsync(systemPrompt, userPrompt, jsonMode: false, cancellationToken);

        public async Task<T?> GenerateJsonAsync<T>(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
        {
            var content = await SendChatAsync(systemPrompt, userPrompt, jsonMode: true, cancellationToken);
            if (string.IsNullOrWhiteSpace(content))
                return default;

            try
            {
                var cleaned = StripMarkdownFences(content);
                return JsonSerializer.Deserialize<T>(cleaned, JsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize Ollama JSON output: {Content}", content);
                return default;
            }
        }

        private async Task<string?> SendChatAsync(string systemPrompt, string userPrompt, bool jsonMode, CancellationToken cancellationToken)
        {
            var payload = new
            {
                model = _model,
                stream = false,
                format = jsonMode ? "json" : (string?)null,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                }
            };

            try
            {
                using var requestTimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                requestTimeoutCts.CancelAfter(_timeout);

                using var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}{CHAT_ENDPOINT}")
                {
                    Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
                };

                using var response = await _httpClient.SendAsync(request, requestTimeoutCts.Token);
                var responseBody = await response.Content.ReadAsStringAsync(requestTimeoutCts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Ollama returned {StatusCode}: {Body}", response.StatusCode, responseBody);
                    return null;
                }

                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                if (root.TryGetProperty("error", out var errorElement))
                {
                    _logger.LogWarning("Ollama error response: {Error}", errorElement.GetString());
                    return null;
                }

                if (!root.TryGetProperty("message", out var messageElement) ||
                    !messageElement.TryGetProperty("content", out var contentElement))
                {
                    _logger.LogWarning("Ollama response missing 'message.content'");
                    return null;
                }

                var content = contentElement.GetString();
                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning("Ollama returned empty content");
                    return null;
                }

                return content;
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Ollama request timed out");
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Ollama request failed - is Ollama running at {BaseUrl}?", _baseUrl);
                return null;
            }
        }

        /// <summary>
        /// Removes triple-backtick fences some models wrap JSON replies in.
        /// </summary>
        private static string StripMarkdownFences(string content)
        {
            var trimmed = content.Trim();
            if (trimmed.StartsWith("```", StringComparison.Ordinal))
            {
                var firstNewLine = trimmed.IndexOf('\n');
                if (firstNewLine >= 0)
                    trimmed = trimmed[(firstNewLine + 1)..];
                trimmed = trimmed.TrimEnd('`', '\r', '\n', ' ').Trim();
            }
            return trimmed;
        }
    }
}