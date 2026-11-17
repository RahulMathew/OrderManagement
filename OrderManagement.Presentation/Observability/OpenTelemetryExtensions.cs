using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace OrderManagement.Presentation.Observability
{
    public static class OpenTelemetryExtensions
    {
        public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration)
        {
            const string serviceName = "OrderManagement.API";

            // Read settings loaded from Azure Key Vault in Program.cs
            string? appInsightsConnectionString = configuration["ApplicationInsights:ConnectionString"];

            string? otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"];

            // Configure logging options separately.
            // LoggerProviderBuilder does not support Configure().
            services.Configure<OpenTelemetryLoggerOptions>(options =>
            {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.ParseStateValues = true;
            });

            services.AddOpenTelemetry()
                .ConfigureResource(resource =>
                {
                    resource.AddService(serviceName);
                })
                // ==========================================
                // Distributed Tracing
                // ==========================================
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()

                        // Custom activities
                        .AddSource("OrderManagement.Application")
                        .AddSource("OrderManagement.Infrastructure");

                    // OpenTelemetry Collector
                    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                    {
                        tracing.AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(otlpEndpoint);
                        });
                    }

                    // Azure Application Insights
                    if (!string.IsNullOrWhiteSpace(
                        appInsightsConnectionString))
                    {
                        tracing.AddAzureMonitorTraceExporter(options =>
                        {
                            options.ConnectionString =
                                appInsightsConnectionString;
                        });
                    }
                })
                // ==========================================
                // Metrics
                // ==========================================
                .WithMetrics(metrics =>
                {
                    metrics
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        // Custom business metrics
                        .AddMeter("OrderManagement.Application")
                        .AddMeter("OrderManagement.Infrastructure");

                    // OpenTelemetry Collector
                    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                    {
                        metrics.AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(otlpEndpoint);
                        });
                    }

                    // Azure Application Insights
                    if (!string.IsNullOrWhiteSpace(
                        appInsightsConnectionString))
                    {
                        metrics.AddAzureMonitorMetricExporter(options =>
                        {
                            options.ConnectionString =
                                appInsightsConnectionString;
                        });
                    }
                })
                // ==========================================
                // Logging
                // ==========================================
                .WithLogging(logging =>
                {
                    // OpenTelemetry Collector
                    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                    {
                        logging.AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(otlpEndpoint);
                        });
                    }

                    // Azure Application Insights
                    if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
                    {
                        logging.AddAzureMonitorLogExporter(options =>
                        {
                            options.ConnectionString = appInsightsConnectionString;
                        });
                    }
                });

            return services;
        }
    }
}
