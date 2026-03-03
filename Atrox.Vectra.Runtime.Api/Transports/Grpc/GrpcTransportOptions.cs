namespace Atrox.Vectra.Runtime.Api.Transports.Grpc
{
    public class GrpcTransportOptions
    {
        public bool Enabled { get; set; }
        public int Port { get; set; } = 5001;
        public int TlsPort { get; set; }
    }
}
