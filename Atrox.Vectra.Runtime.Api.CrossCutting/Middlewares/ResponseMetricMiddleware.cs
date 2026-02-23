namespace CrossCutting.Middlewares
{
    using CrossCutting.Metrics;

    using Microsoft.AspNetCore.Http;

    using System.Diagnostics;

    public class ResponseMetricMiddleware(RequestDelegate request)
    {
        private readonly RequestDelegate _request = request ?? throw new ArgumentNullException(nameof(request));

        public async Task Invoke(HttpContext httpContext, MetricCollector collector)
        {
            var path = httpContext.Request.Path.Value;

            if (path == "/metrics")
            {
                await _request.Invoke(httpContext);

                return;
            }

            var sw = Stopwatch.StartNew();

            try
            {
                await _request.Invoke(httpContext);
            }
            finally
            {
                sw.Stop();

                collector.RegisterRequest();
                collector.RegisterResponseTime(httpContext.Response.StatusCode, httpContext.Request.Method, sw.Elapsed);
            }
        }
    }
}
