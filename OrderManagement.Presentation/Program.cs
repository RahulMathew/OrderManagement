using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi;
using OrderManagement.Application.Commands.Order;
using OrderManagement.Infrastructure.DIRegistrations;
using OrderManagement.Presentation.Middlewares;

namespace OrderManagement.Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddScoped<SecurityMiddleware>();

            // 1.Microsoft Entra ID / Azure AD Authentication and load secrets from Azure Key Vault
            string? clientId = null;
            string? tenantId = null;
            string? kafkaBootstrapServers = null;
            string? kafkaTopic = null;
            string? dbConnectionString = null;

            string? keyVaultUrl = builder.Configuration["KeyVault:Url"];
            if (!string.IsNullOrEmpty(keyVaultUrl))
            {
                var credential = new DefaultAzureCredential();
                builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), credential);

                var secretClient = new SecretClient(new Uri(keyVaultUrl), credential);
                clientId = GetSecretValue(secretClient, "ClientId");
                tenantId = GetSecretValue(secretClient, "TenantId");
                
                // Read Kafka settings from Azure Key Vault
                kafkaBootstrapServers = GetSecretValue(secretClient, "Kafka--BootstrapServers");
                kafkaTopic = GetSecretValue(secretClient, "Kafka--Topic");
                dbConnectionString = GetSecretValue(secretClient, "DbConnectionString");
            }

            var azureAd = builder.Configuration.GetSection("AzureAd");
            if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(tenantId))
            {
                azureAd["ClientId"] = clientId;
                azureAd["TenantId"] = tenantId;
            }

            // Update Kafka configuration
            if (!string.IsNullOrEmpty(kafkaBootstrapServers))
            {
                builder.Configuration["Kafka:BootstrapServers"] =
                    kafkaBootstrapServers;
            }

            if (!string.IsNullOrEmpty(kafkaTopic))
            {
                builder.Configuration["Kafka:Topic"] = kafkaTopic;
            }

            // Update SQL Server connection string from Azure Key Vault
            if (!string.IsNullOrWhiteSpace(dbConnectionString))
            {
                builder.Configuration["ConnectionStrings:DefaultConnection"] =
                    dbConnectionString;
            }

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

                options.AddSecurityDefinition(
                    "Bearer",
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

            var app = builder.Build();

            // 6. Swagger
            if (app.Configuration.GetValue<bool>(
                "EnableSwagger", true))
            {
                app.UseSwagger();

                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint(
                        "/swagger/v1/swagger.json",
                        "OrderManagement API v1");
                });
            }

            // 7. Middleware
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            // 8. Controllers
            app.MapControllers();

            app.Run();
        }

        private static string? GetSecretValue(SecretClient client, params string[] secretNames)
        {
            foreach (var secretName in secretNames)
            {
                try
                {
                    var secretValue = client.GetSecret(secretName).Value.Value;

                    if (!string.IsNullOrWhiteSpace(secretValue))
                    {
                        return secretValue;
                    }
                }
                catch (RequestFailedException)
                {
                    // Secret not found in Key Vault; continue checking alternate names.
                }
            }

            return null;
        }
    }
}


