using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi;
using OrderManagement.Application.Commands.Order;
using OrderManagement.Infrastructure.DIRegistrations;
using OrderManagement.Presentation.ExceptionHandling;
using OrderManagement.Presentation.Middlewares;
using OrderManagement.Presentation.Observability;

namespace OrderManagement.Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddScoped<SecurityMiddleware>();

            // 1. Microsoft Entra ID / Azure AD Authentication
            // and load secrets from Azure Key Vault
            string? clientId = null;
            string? tenantId = null;
            string? kafkaBootstrapServers = null;
            string? kafkaTopic = null;
            string? dbConnectionString = null;
            string? appInsightsConnectionString = null;
            string? otlpEndpoint = null;
            string? defaultLogLevel = null;
            string? applicationLogLevel = null;

            string? keyVaultUrl = builder.Configuration["KeyVault:Url"];

            if (!string.IsNullOrWhiteSpace(keyVaultUrl))
            {
                var credential = new DefaultAzureCredential();

                builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), credential);

                var secretClient = new SecretClient(new Uri(keyVaultUrl), credential);

                clientId = GetSecretValue(secretClient, "ClientId");
                tenantId = GetSecretValue(secretClient, "TenantId");

                // Read Kafka settings from Azure Key Vault
                kafkaBootstrapServers = GetSecretValue(secretClient, "Kafka--BootstrapServers");

                kafkaTopic = GetSecretValue(secretClient, "Kafka--Topic");

                // Read SQL Server connection string from Azure Key Vault
                dbConnectionString = GetSecretValue(secretClient, "DbConnectionString");

                // Application Insights
                appInsightsConnectionString = GetSecretValue(secretClient, "ApplicationInsights--ConnectionString");

                // OpenTelemetry Collector (optional)
                otlpEndpoint = GetSecretValue(secretClient, "OpenTelemetry--OtlpEndpoint");

                // Read logging levels from Azure Key Vault
                defaultLogLevel = GetSecretValue(secretClient, "Logging--LogLevel--Default");

                applicationLogLevel = GetSecretValue(secretClient, "Logging--LogLevel--OrderManagement");
            }

            var azureAd = builder.Configuration.GetSection("AzureAd");

            if (!string.IsNullOrWhiteSpace(clientId) &&
                !string.IsNullOrWhiteSpace(tenantId))
            {
                azureAd["ClientId"] = clientId;
                azureAd["TenantId"] = tenantId;
            }

            // Update Kafka configuration
            if (!string.IsNullOrWhiteSpace(kafkaBootstrapServers))
                builder.Configuration["Kafka:BootstrapServers"] = kafkaBootstrapServers;

            if (!string.IsNullOrWhiteSpace(kafkaTopic))
                builder.Configuration["Kafka:Topic"] = kafkaTopic;

            // Update SQL Server connection string from Azure Key Vault
            if (!string.IsNullOrWhiteSpace(dbConnectionString))
                builder.Configuration["ConnectionStrings:DefaultConnection"] = dbConnectionString;

            // Application Insights configuration
            if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
                builder.Configuration["ApplicationInsights:ConnectionString"] = appInsightsConnectionString;

            // OpenTelemetry Collector configuration
            if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                builder.Configuration["OpenTelemetry:OtlpEndpoint"] = otlpEndpoint;

            // Logging configuration

            // Default to Error if Key Vault value is missing or invalid
            if (!Enum.TryParse<LogLevel>(defaultLogLevel,
                    ignoreCase: true,
                    out var logLevel)
                || !Enum.IsDefined(logLevel))
            {
                logLevel = LogLevel.Error;
            }

            // Application-specific logging level
            // Default to the configured default level if missing or invalid
            if (!Enum.TryParse<LogLevel>(applicationLogLevel,
                    ignoreCase: true,
                    out var appLogLevel)
                || !Enum.IsDefined(appLogLevel))
            {
                appLogLevel = logLevel;
            }

            // Update application logging configuration
            builder.Configuration["Logging:LogLevel:Default"] = logLevel.ToString();

            builder.Configuration["Logging:LogLevel:OrderManagement"] = appLogLevel.ToString();

            // Configure OpenTelemetry and Application Insights
            builder.Services.AddObservability(builder.Configuration);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(azureAd);

            builder.Services.AddAuthorization();

            // 2. Controllers
            builder.Services.AddControllers();

            // 3. MediatR - CQRS Commands and Queries
            builder.Services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(
                    typeof(CreateOrderCommand).Assembly));

            // 4. Infrastructure - EF Core, Repositories,
            // UnitOfWork, Idempotency, Outbox and Kafka
            builder.Services.AddInfrastructure(builder.Configuration);

            // 5. Swagger
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "OrderManagement API",
                    Version = "v1"
                });

                options.AddSecurityDefinition("Bearer",
                    new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description =
                            "JWT Authorization header using the Bearer scheme."
                    });
            });

            // 6. Global Exception Handler
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            var app = builder.Build();

            // 7. Global Exception Handling Middleware
            app.UseExceptionHandler();

            // 8. Swagger
            if (app.Configuration.GetValue<bool>("EnableSwagger", true))
            {
                app.UseSwagger();

                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint(
                        "/swagger/v1/swagger.json",
                        "OrderManagement API v1");
                });
            }

            // 9. Middleware
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            // 10. Controllers
            app.MapControllers();

            app.Run();
        }

        private static string? GetSecretValue(SecretClient client, params string[] secretNames)
        {
            foreach (var secretName in secretNames)
            {
                try
                {
                    var secretValue = client
                        .GetSecret(secretName)
                        .Value.Value;

                    if (!string.IsNullOrWhiteSpace(secretValue))
                    {
                        return secretValue;
                    }
                }
                catch (RequestFailedException ex)
                    when (ex.Status == 404)
                {
                    // Secret not found in Key Vault;
                    // continue checking alternate names.
                }
            }

            return null;
        }
    }
}
