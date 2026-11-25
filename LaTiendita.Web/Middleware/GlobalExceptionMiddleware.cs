using System.Net;
using System.Text.Json;

namespace LaTiendita.Web.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Excepción no controlada en {Method} {Path} (TraceId: {TraceId})",
                    context.Request.Method,
                    context.Request.Path,
                    context.TraceIdentifier);

                // Para rutas API devolvemos siempre JSON uniforme; para vistas MVC dejamos actuar a /Home/Error
                if (!context.Request.Path.StartsWithSegments("/api"))
                {
                    throw;
                }

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var respuesta = new
                {
                    exito = false,
                    status = context.Response.StatusCode,
                    mensaje = "Ocurrió un error interno en el servidor.",
                    traceId = context.TraceIdentifier,
                    detalle = _env.IsDevelopment() ? ex.Message : null
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(respuesta));
            }
        }
    }
}
