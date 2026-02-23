namespace CrossCutting.Models.HealthCheckModels
{
    internal class HealthCheckReponse
    {
        public string Status { get; set; }
        public IEnumerable<IndividualHealthCheckResponse> HealthChecks { get; set; }
        public string HealthCheckDuration { get; set; }
    }
}
