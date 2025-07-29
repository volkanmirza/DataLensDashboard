using DataLens.Services;

namespace DataLens.Middleware
{
    public class JwtCookieMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtCookieMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IJwtService jwtService)
        {
            // Check if Authorization header is already present
            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                // Try to get JWT token from cookie
                if (context.Request.Cookies.TryGetValue("jwt_token", out var token))
                {
                    // Validate the token
                    if (jwtService.IsTokenValid(token))
                    {
                        // Add the token to Authorization header
                        context.Request.Headers["Authorization"] = $"Bearer {token}";
                    }
                    else
                    {
                        // Remove invalid token cookie
                        context.Response.Cookies.Delete("jwt_token");
                    }
                }
            }

            await _next(context);
        }
    }

    public static class JwtCookieMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtCookie(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtCookieMiddleware>();
        }
    }
}