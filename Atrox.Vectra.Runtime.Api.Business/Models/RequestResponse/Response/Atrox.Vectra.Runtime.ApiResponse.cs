
namespace Atrox.Vectra.Runtime.Api.Business.Models.RequestResponse.Response
{
    public class AtroxVectraRuntimeApiResponse
    {
        public int returns { get; set; }
        public List<string> prints { get; set; }
        public List<RaisError> raisError { get; set; }
        public Dictionary<string, ParameterValue> outputParameters { get; set; }
        public List<ResultSets> resultSets { get; set; }
    }
}



