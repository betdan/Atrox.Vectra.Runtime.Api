namespace Atrox.Vectra.Runtime.Api.Metrics
{
    using App.Metrics;
    using App.Metrics.Counter;

    public class MetricsRegistry
    {
        public static CounterOptions CreatedApplicationsCounter => new CounterOptions
        {
            Name = "Created Applications",
            Context = "Atrox.Vectra.Runtime.Api.Controller",
            MeasurementUnit = Unit.Calls
        };
    }
}



