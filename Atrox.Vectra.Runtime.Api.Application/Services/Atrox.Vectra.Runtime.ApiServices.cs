namespace Atrox.Vectra.Runtime.Api.Application.Services
{
    using Microsoft.Extensions.Logging;
    using Atrox.Vectra.Runtime.Api.Application.Contracts.Services;
    using Atrox.Vectra.Runtime.Api.DataAccess.Contracts.Executions;
    using Atrox.Vectra.Runtime.Api.Business.Models.RequestResponse.Request;
    using Atrox.Vectra.Runtime.Api.Business.Models.RequestResponse.Response;

    public class AtroxVectraRuntimeApiServices(ILogger<AtroxVectraRuntimeApiServices> logger, IAtroxVectraRuntimeApiExecute db) : IAtroxVectraRuntimeApiServices
    {
        private readonly ILogger<AtroxVectraRuntimeApiServices> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IAtroxVectraRuntimeApiExecute _db = db ?? throw new ArgumentNullException(nameof(db));

        public async Task<ServiceResult> ExecuteServiceAsync(ApplicationDto req)
        {
            ServiceResult result = new ServiceResult();
            var dataAccessResult = await _db.ExecuteAtroxVectraRuntimeApiClientAsync(req);
            result.Data = dataAccessResult;

            List<ErrorDto> errors = new List<ErrorDto>();

            foreach (var item in dataAccessResult.raisError)
            {
                errors.Add(new ErrorDto
                {
                    Code = item.code.ToString(),
                    Message = item.message
                });
            }

            result.Error = errors;
            return result;
        }
    }
}




