using System.Text;
using Microsoft.AspNetCore.Http;

namespace TimeTracker.Business.Extensions
{
    public static class HttpRequestExtension
    {   
        public static string? GetToken(this HttpRequest request)
        {
            var authHeader = request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            return string.IsNullOrEmpty(token) ? null : token;
        }

        public static string? GetBearerToken(this HttpRequest request) => request.GetToken();

        public static async Task<string> ReadBodyAsync(this HttpRequest request)
        {
            var result = "";
            try
            {
                request.EnableBuffering();
                // Arguments: Stream, Encoding, detect encoding, buffer size 
                // AND, the most important: keep stream opened
                using (var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
                {
                    result = await reader.ReadToEndAsync();
                }
            }
            finally
            {
                // Rewind, so the core is not lost when it looks the body for the request
                request.Body.Position = 0;
            }
            return result;
        }
    }
}
