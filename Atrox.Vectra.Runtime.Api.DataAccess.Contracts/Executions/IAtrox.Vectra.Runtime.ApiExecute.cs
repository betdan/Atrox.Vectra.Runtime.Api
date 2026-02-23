namespace Atrox.Vectra.Runtime.Api.DataAccess.Contracts.Executions
{
    using Atrox.Vectra.Runtime.Api.Business.Models.RequestResponse.Request;
    using Atrox.Vectra.Runtime.Api.Business.Models.RequestResponse.Response;

    public interface IAtroxVectraRuntimeApiExecute
    {
        Task<AtroxVectraRuntimeApiResponse> ExecuteAtroxVectraRuntimeApiClientAsync(ApplicationDto req);
    }
}




