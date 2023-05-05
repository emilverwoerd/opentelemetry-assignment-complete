using Metrics.NET.Metrics;
using Metrics.NET.Metrics.Config;
using Metrics.NET.Middleware;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IComputerComponentsMetrics, ComputerComponentsMetrics>();

builder.Services.AddOpenTelemetry()
    .WithMetrics(builder => builder
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(DiagnosticsConfig.ServiceName))
        .AddMeter(DiagnosticsConfig.ComputerComponentsMeterName)
        .AddView(
            instrumentName: "components-per-order",
            new ExplicitBucketHistogramConfiguration { Boundaries = new double[] { 1, 2, 5, 10 } })
        .AddConsoleExporter()
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter()
        .AddPrometheusExporter());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseOpenTelemetryPrometheusScrapingEndpoint();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add simulated latency to improve http requests avg. time dashboard
app.UseSimulatedLatency(
    min: TimeSpan.FromMilliseconds(500),
    max: TimeSpan.FromMilliseconds(1000)
);

app.UseAuthorization();
app.MapControllers();
app.Run();
