namespace CommentsRatingsAPI
{
    public class ApiRequestMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiRequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Extract the correlation ID from the request
            var correlationId = context.Request.Headers["X-Correlation-ID"].ToString();

            // Check if the API call is from your own APIs or user interface
            // This can be based on a specific header, IP address, or any other logic
            var isInternalCall = IsInternalApiCall(context);

            if (string.IsNullOrEmpty(correlationId) && isInternalCall)
            {
                // Generate a new correlation ID if it's missing in an internal call
                correlationId = Guid.NewGuid().ToString();
                context.Request.Headers["X-Correlation-ID"] = correlationId;
            }

            // Pass the correlation ID to your message (RabbitMQ or other)
            // ...

            await _next(context);
        }

        private bool IsInternalApiCall(HttpContext context)
        {
            // Implement your logic to determine if the request is from your own APIs or UI
            // For example, checking a custom header or the source IP address
            // Return true if it's an internal call, false otherwise
            return context.Request.Headers.ContainsKey("X-Internal-Api");
        }
    }
}
