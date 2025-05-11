using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FleetTracking.Middleware
{
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
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

            var statusCode = HttpStatusCode.InternalServerError;

            // Customize status code based on exception type
            if (exception is ArgumentException || exception is FormatException)
            {
                statusCode = HttpStatusCode.BadRequest;
            }
            else if (exception is UnauthorizedAccessException)
            {
                statusCode = HttpStatusCode.Unauthorized;
            }
            else if (exception is KeyNotFoundException)
            {
                statusCode = HttpStatusCode.NotFound;
            }
            
            // If this is an API request (Accept header contains application/json or request path starts with /api)
            var isApiRequest = context.Request.Headers["Accept"].ToString().Contains("application/json") 
                              || context.Request.Path.StartsWithSegments("/api");

            if (isApiRequest)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)statusCode;

                var result = JsonSerializer.Serialize(new 
                { 
                    error = true, 
                    message = "An error occurred while processing your request", 
                    detailedMessage = context.Request.Host.Value.Contains("localhost") ? exception.Message : null
                });

                return context.Response.WriteAsync(result);
            }
            else
            {
                // For regular web requests, redirect to custom error page
                context.Response.Redirect($"/Home/Error?statusCode={(int)statusCode}");
                return Task.CompletedTask;
            }
        }
    }
} 