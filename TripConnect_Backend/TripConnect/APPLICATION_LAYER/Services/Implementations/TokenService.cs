using APPLICATION_LAYER.DTOs.Auth;
using APPLICATION_LAYER.Models;
using APPLICATION_LAYER.Services.Interfaces;
using DOMAIN_LAYER.Entity.User;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace APPLICATION_LAYER.Services.Implementations
{
    /// <summary>
    /// Service for generating and validating JWT tokens
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger _logger;

        public TokenService(IOptions<JwtSettings> jwtSettings, ILogger logger)
        {
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Generates a JWT access token for the given user
        /// </summary>
        public JwtTokenResponseDto GenerateAccessToken(User user)
        {
            try
            {
                var securityKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                // Create claims for the JWT token
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("FullName", user.Name),
                    new Claim("Phone", user.Phone ?? string.Empty)
                };

                var issuedAt = DateTime.UtcNow;
                var expiresAt = issuedAt.AddMinutes(_jwtSettings.ExpirationMinutes);

                // Create the JWT token
                var token = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims,
                    notBefore: issuedAt,
                    expires: expiresAt,
                    signingCredentials: credentials
                );

                var tokenHandler = new JwtSecurityTokenHandler();
                var accessToken = tokenHandler.WriteToken(token);

                _logger.Information($"Access token generated for user: {user.Id}");

                return new JwtTokenResponseDto
                {
                    AccessToken = accessToken,
                    TokenType = "Bearer",
                    ExpiresIn = (int)expiresAt.Subtract(issuedAt).TotalSeconds,
                    IssuedAt = issuedAt,
                    ExpiresAt = expiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"Error generating access token for user {user.Id}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Generates a cryptographically secure refresh token
        /// </summary>
        public string GenerateRefreshToken()
        {
            try
            {
                // Generate a random 32-byte array and convert to Base64
                var randomNumber = new byte[32];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomNumber);
                    var refreshToken = Convert.ToBase64String(randomNumber);
                    _logger.Information("Refresh token generated");
                    return refreshToken;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error generating refresh token: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Extracts claims from an expired JWT token (ignoring expiration)
        /// </summary>
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            try
            {
                var securityKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey,
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = false, // Ignore expiration when refreshing
                    ClockSkew = TimeSpan.Zero
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, 
                    out SecurityToken securityToken);

                if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.Warning("Invalid token algorithm");
                    throw new SecurityTokenException("Invalid token");
                }

                _logger.Information("Principal extracted from expired token");
                return principal;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error extracting principal from token: {ex.Message}");
                throw;
            }
        }
    }
}
