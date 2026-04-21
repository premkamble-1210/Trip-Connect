using System.Net;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace API_LAYER.Middleware
{
    /// <summary>
    /// Error Handling Middleware - Global exception handling and error response formatting
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unhandled exception: {ex.Message}\n{ex.StackTrace}");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = new { success = false, message = "", errorCode = "" };

            switch (exception)
            {
                // JWT Token Exceptions
                case SecurityTokenExpiredException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = new { success = false, message = "Token has expired", errorCode = "TOKEN_EXPIRED" };
                    break;

                case SecurityTokenInvalidSignatureException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = new { success = false, message = "Invalid token signature", errorCode = "INVALID_SIGNATURE" };
                    break;

                case SecurityTokenValidationException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = new { success = false, message = "Token validation failed", errorCode = "TOKEN_INVALID" };
                    break;

                case SecurityTokenException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = new { success = false, message = "Security token error", errorCode = "TOKEN_ERROR" };
                    break;

                // Standard Exceptions
                case InvalidOperationException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new { success = false, message = exception.Message, errorCode = "INVALID_OPERATION" };
                    break;

                case KeyNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response = new { success = false, message = "Resource not found", errorCode = "NOT_FOUND" };
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = new { success = false, message = "Unauthorized access", errorCode = "UNAUTHORIZED" };
                    break;

                case ArgumentException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new { success = false, message = exception.Message, errorCode = "INVALID_ARGUMENT" };
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response = new { success = false, message = "Internal server error", errorCode = "INTERNAL_ERROR" };
                    break;
            }

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
