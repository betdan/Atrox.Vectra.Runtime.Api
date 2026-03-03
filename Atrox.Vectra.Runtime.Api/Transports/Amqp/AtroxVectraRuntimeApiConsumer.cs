namespace Atrox.Vectra.Runtime.Api.Transports.Amqp
{
    using global::MassTransit;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Atrox.Vectra.Runtime.Api.Application.Contracts.Services;
    using Atrox.Vectra.Runtime.Api.Business.Models.RequestResponse.Response;
    using Atrox.Vectra.Runtime.Api.MassTransit.Contracts;

    public class AtroxVectraRuntimeApiConsumer(ILogger<AtroxVectraRuntimeApiConsumer> logger, IExecutionService executionService) : IConsumer<MtEvent>
    {
        private readonly ILogger<AtroxVectraRuntimeApiConsumer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IExecutionService _executionService = executionService ?? throw new ArgumentNullException(nameof(executionService));

        public async Task Consume(ConsumeContext<MtEvent> context)
        {
            ServiceResult response = new ServiceResult();
            var request = context.Message.Req;
            _logger.LogDebug("AMQP runtime request: {request}", JsonConvert.SerializeObject(request, Formatting.Indented));

            response = await _executionService.ExecuteServiceAsync(request);

            _logger.LogDebug("AMQP runtime response: {response}", JsonConvert.SerializeObject(response, Formatting.Indented));

            var resultEvent = new MtResultEvent(context.Message.CorrelationId, DateTime.Now, response);
            await context.RespondAsync<MtResultEvent>(resultEvent);
        }
    }
}
