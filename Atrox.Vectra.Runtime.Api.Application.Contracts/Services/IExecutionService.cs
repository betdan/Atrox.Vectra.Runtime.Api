namespace Atrox.Vectra.Runtime.Api.Application.Contracts.Services
{
    using Atrox.Vectra.Runtime.Api.Business.Models.RequestResponse.Request;
    using Atrox.Vectra.Runtime.Api.Business.Models.RequestResponse.Response;

    public interface IExecutionService
    {
        Task<ServiceResult> ExecuteServiceAsync(ApplicationDto req);
    }
}
