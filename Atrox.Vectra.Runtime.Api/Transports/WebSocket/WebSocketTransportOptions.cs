namespace Atrox.Vectra.Runtime.Api.Transports.WebSocket
{
    public class WebSocketTransportOptions
    {
        public bool Enabled { get; set; }
        public string Path { get; set; } = "/ws/runtime";
        public int KeepAliveSeconds { get; set; } = 120;
    }
}
