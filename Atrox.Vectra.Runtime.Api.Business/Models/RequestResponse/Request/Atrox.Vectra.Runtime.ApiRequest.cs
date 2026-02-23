namespace Atrox.Vectra.Runtime.Api.Business.Models.RequestResponse.Request
{
    public class AtroxVectraRuntimeApiRequest
    {
        public String databaseName { get; set; }
        public String procedureName { get; set; }
        public List<InputParamsRequest> inputParameters { get; set; }
    }
}




