namespace Atrox.Vectra.Runtime.Api.Business.Models.RequestResponse.Response
{
    public class ServiceResult
    {
        public AtroxVectraRuntimeApiResponse Data { get; set; }
        public List<ErrorDto> Error { get; set; }
    }
}




