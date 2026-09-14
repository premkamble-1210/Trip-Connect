using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace INFRASTRUCTURE_LAYER.Services
{
    /// <summary>
    /// Implementation of <see cref="IBannerService"/> using the free
    /// pollinations.ai image generation API (no API key required).
    /// </summary>
    public class BannerService : IBannerService
    {
        private readonly ILogger<BannerService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly int _width;
        private readonly int _height;
        private readonly TimeSpan _timeout;

        public BannerService(IConfiguration configuration, ILogger<BannerService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;

            _baseUrl = (configuration["Banner:BaseUrl"] ?? "https://image.pollinations.ai").TrimEnd('/');
            _width = int.TryParse(configuration["Banner:Width"], out var w) ? w : 1280;
            _height = int.TryParse(configuration["Banner:Height"], out var h) ? h : 720;

            var timeoutSeconds = int.TryParse(configuration["Banner:TimeoutSeconds"], out var timeout)
                ? timeout
                : 90;
            _timeout = TimeSpan.FromSeconds(timeoutSeconds);

            _logger.LogInformation("Banner service configured. BaseUrl: {BaseUrl}, Size: {Width}x{Height}, Timeout: {Timeout}s",
                _baseUrl, _width, _height, timeoutSeconds);
        }

        public string BuildBannerUrl(string prompt)
        {
            var encodedPrompt = Uri.EscapeDataString(prompt);
            var seed = Random.Shared.Next(0, 1_000_000);
            return $"{_baseUrl}/prompt/{encodedPrompt}?width={_width}&height={_height}&nologo=true&seed={seed}";
        }

        public async Task<byte[]?> DownloadBannerImageAsync(string prompt, CancellationToken cancellationToken = default)
        {
            var url = BuildBannerUrl(prompt);
            try
            {
                using var requestTimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                requestTimeoutCts.CancelAfter(_timeout);

                using var response = await _httpClient.GetAsync(url, requestTimeoutCts.Token);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Banner generation returned {StatusCode} for prompt", response.StatusCode);
                    return null;
                }

                var bytes = await response.Content.ReadAsByteArrayAsync(requestTimeoutCts.Token);
                if (bytes.Length == 0)
                {
                    _logger.LogWarning("Banner generation returned empty content for prompt");
                    return null;
                }

                _logger.LogInformation("Banner generated successfully ({Size} bytes)", bytes.Length);
                return bytes;
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Banner generation timed out");
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Banner generation request failed");
                return null;
            }
        }
    }
}