using DatingAppApi.Errors;
using System.Text.Json;
using System.Net;
using System.Globalization;
using DatingAppApi.Middleware;

namespace DatingAppApi.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _env = env;
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                 await _next(context);

                //var cultureQuery = context.Request.Query["culture"];
                //if (!string.IsNullOrWhiteSpace(cultureQuery))
                //{
                //    var culture = new CultureInfo(cultureQuery);

                //    CultureInfo.CurrentCulture = culture;
                //    CultureInfo.CurrentUICulture = culture;
                //}

                //// Call the next delegate/middleware in the pipeline.
                //await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = _env.IsDevelopment()
                    ? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
                    : new ApiException(context.Response.StatusCode, ex.Message, "Internal Server Error");

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);
            }
        }
    }
}


//public static class RequestCultureMiddlewareExtensions
//{
//    public static IApplicationBuilder UseRequestCulture(
//        this IApplicationBuilder builder)
//    {
//        return builder.UseMiddleware<ExceptionMiddleware>();
//    }
//}
