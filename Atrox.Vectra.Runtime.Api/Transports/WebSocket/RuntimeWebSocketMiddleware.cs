namespace Atrox.Vectra.Runtime.Api.Transports.WebSocket
{
    using CrossCutting.CanonicalSignature;
    using Atrox.Vectra.Runtime.Api.Application.Contracts.Services;
    using Atrox.Vectra.Runtime.Api.Business.Models.RequestResponse.Request;
    using Atrox.Vectra.Runtime.Api.Business.Models.RequestResponse.Response;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using System.Net.WebSockets;
    using System.Text;
    using System.Text.Json;

    public class RuntimeWebSocketMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory, IOptions<WebSocketTransportOptions> transportOptions, ILogger<RuntimeWebSocketMiddleware> logger)
    {
        private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        private readonly WebSocketTransportOptions _transportOptions = transportOptions?.Value ?? throw new ArgumentNullException(nameof(transportOptions));
        private readonly ILogger<RuntimeWebSocketMiddleware> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task InvokeAsync(HttpContext context)
        {
            if (!_transportOptions.Enabled || !context.Request.Path.Equals(_transportOptions.Path, StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("WebSocket request expected.");
                return;
            }

            using var socket = await context.WebSockets.AcceptWebSocketAsync();
            _logger.LogInformation("WebSocket runtime client connected from {remoteIp}.", context.Connection.RemoteIpAddress);
            await HandleConnectionAsync(socket, context.RequestAborted);
        }

        private async Task HandleConnectionAsync(System.Net.WebSockets.WebSocket socket, CancellationToken cancellationToken)
        {
            var buffer = new byte[4 * 1024];

            while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var messageBuilder = new StringBuilder();
                WebSocketReceiveResult receiveResult;

                do
                {
                    receiveResult = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client.", cancellationToken);
                        return;
                    }

                    messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, receiveResult.Count));
                }
                while (!receiveResult.EndOfMessage);

                var requestPayload = messageBuilder.ToString();
                _logger.LogDebug("WebSocket runtime request: {request}", requestPayload);
                var responsePayload = await ProcessMessageAsync(requestPayload);
                _logger.LogDebug("WebSocket runtime response: {response}", responsePayload);

                var responseBytes = Encoding.UTF8.GetBytes(responsePayload);
                await socket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, cancellationToken);
            }
        }

        private async Task<string> ProcessMessageAsync(string requestPayload)
        {
            try
            {
                var serializerOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var serviceRequest = JsonSerializer.Deserialize<ServiceRequest<ApplicationDto>>(requestPayload, serializerOptions);
                var executionRequest = serviceRequest?.Body ?? JsonSerializer.Deserialize<ApplicationDto>(requestPayload, serializerOptions);

                if (executionRequest?.Data == null)
                {
                    var invalidPayloadResponse = new ServiceResult
                    {
                        Data = null,
                        Error = new List<ErrorDto>
                        {
                            new()
                            {
                                Code = "WS_INVALID_PAYLOAD",
                                Message = "Invalid request payload."
                            }
                        }
                    };

                    return JsonSerializer.Serialize(invalidPayloadResponse);
                }

                using var scope = _scopeFactory.CreateScope();
                var executionService = scope.ServiceProvider.GetRequiredService<IExecutionService>();
                var executionResponse = await executionService.ExecuteServiceAsync(executionRequest);
                _logger.LogDebug("WebSocket runtime execution response object: {response}", Newtonsoft.Json.JsonConvert.SerializeObject(executionResponse, Newtonsoft.Json.Formatting.Indented));
                return JsonSerializer.Serialize(executionResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WebSocket runtime message processing failed.");

                var errorResponse = new ServiceResult
                {
                    Data = null,
                    Error = new List<ErrorDto>
                    {
                        new()
                        {
                            Code = "WS_EXECUTION_ERROR",
                            Message = ex.Message
                        }
                    }
                };

                return JsonSerializer.Serialize(errorResponse);
            }
        }
    }
}
