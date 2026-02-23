namespace Atrox.Vectra.Runtime.Api.MassTransit.Contracts
{
    using global::MassTransit;
    using Atrox.Vectra.Runtime.Api.Business.Models.RequestResponse.Request;
    using Atrox.Vectra.Runtime.Api.Business.Models.RequestResponse.Response;

    using System.Text.Json.Serialization;

    [JsonSerializable(typeof(ServiceResult))]
    public record MtEvent : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; init; }
        public DateTime FechaSolicitud { get; init; }
        public ApplicationDto Req { get; init; }

        [JsonConstructor]
        public MtEvent(Guid correlationId, DateTime fechaSolicitud, ApplicationDto req)
        {
            CorrelationId = correlationId;
            FechaSolicitud = fechaSolicitud;
            Req = req;
        }
    }

    [JsonSerializable(typeof(MtEvent))]
    public record MtResultEvent : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; init; }
        public DateTime Timestamp { get; init; }
        public ServiceResult Res { get; init; }

        [JsonConstructor]
        public MtResultEvent(Guid correlationId, DateTime timestamp, ServiceResult res)
        {
            CorrelationId = correlationId;
            Timestamp = timestamp;
            Res = res;
        }
    }
}



