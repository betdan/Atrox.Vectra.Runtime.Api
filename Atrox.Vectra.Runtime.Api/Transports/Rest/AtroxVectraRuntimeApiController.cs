namespace Atrox.Vectra.Runtime.Api.Transports.Rest
{
    using CrossCutting.CanonicalSignature;
    using Microsoft.AspNetCore.Mvc;
    using System.Net;
    using System.Text.Json;
    using Atrox.Vectra.Runtime.Api.Application.Contracts.Services;
    using Atrox.Vectra.Runtime.Api.Business.Models.RequestResponse.Request;
    using Atrox.Vectra.Runtime.Api.Business.Models.RequestResponse.Response;

    [Route("api/v1/[controller]")]
    [ApiController]
    public class AtroxVectraRuntimeApiController(ILogger<AtroxVectraRuntimeApiController> logger, IExecutionService executionService) : ControllerBase
    {
        private readonly ILogger<AtroxVectraRuntimeApiController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IExecutionService _executionService = executionService ?? throw new ArgumentNullException(nameof(executionService));

        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ServiceResult))]
        [Produces("application/json")]
        [HttpPost]
        public async Task<IActionResult> DefaultRoute(ServiceRequest<ApplicationDto> req)
        {
            var applicationDto = req.Body;
            _logger.LogInformation("Request received.");
            _logger.LogDebug("Request payload: {requestPayload}", JsonSerializer.Serialize(applicationDto, new JsonSerializerOptions { WriteIndented = true }));

            var response = await _executionService.ExecuteServiceAsync(applicationDto);

            _logger.LogInformation("Request processed successfully.");
            _logger.LogDebug("Response payload: {responsePayload}", JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true }));

            return Ok(response);
        }

        [HttpPost("{extraValue}")]
        public async Task<IActionResult> HandleExtraValue(ServiceRequest<ApplicationDto> req, string extraValue)
        {
            var applicationDto = req.Body;
            _logger.LogInformation("Request received with route parameter: {extraValue}", extraValue);
            _logger.LogDebug("Request payload: {requestPayload}", JsonSerializer.Serialize(applicationDto, new JsonSerializerOptions { WriteIndented = true }));

            var response = await _executionService.ExecuteServiceAsync(applicationDto);

            _logger.LogInformation("Request with route parameter processed successfully.");
            _logger.LogDebug("Response payload: {responsePayload}", JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true }));

            return Ok(response);
        }
    }
}
