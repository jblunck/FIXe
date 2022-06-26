using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

public static class FixGatewayExtensions
{
    public static IServiceCollection AddFixGatewayShell(this IServiceCollection services)
    {
        services.AddOpenTelemetryMetrics(options =>
        {
            options
                .AddAspNetCoreInstrumentation()
                .AddPrometheusExporter();
        });
        return services;
    }

    public static IHostBuilder UseFixGatewayShell(this IHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((hostingContext, config) =>
        {
            var env = hostingContext.HostingEnvironment;
            config.AddJsonFile("appsettings.json");
            config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
            if (env.IsDevelopment())
            {
                config.AddJsonFile($"apsettings.{System.Environment.MachineName}.json", optional: true);
                config.AddUserSecrets<Program>();
            }
            config.AddEnvironmentVariables();

            var settings = config.Build();
            var connString = settings.GetConnectionString("AppConfig");
            if (!string.IsNullOrEmpty(connString))
            {
                var labels = Environment.GetEnvironmentVariable("APPCONFIG_LABELS") ?? "";
                config.AddAzureAppConfiguration(options =>
                {
                    options
                        .Connect(connString)
                        .ConfigureKeyVault(kv =>
                        {
                            kv.SetCredential(new DefaultAzureCredential());
                        })
                        .Select(KeyFilter.Any, null)
                        .Select(KeyFilter.Any, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
                    foreach (var label in labels.Split(",").ToList())
                    {
                        options.Select(KeyFilter.Any, label);
                    }
                }, optional: true);
            }
        });

        builder.ConfigureLogging((hostingContext, builder) =>
        {
            var env = hostingContext.HostingEnvironment;
            builder.ClearProviders();

            if (!env.IsDevelopment() && System.Console.IsInputRedirected)
            {
                builder.AddJsonConsole();
            }
            else
            {
                builder.AddConsole();
            }
        });

        return builder;
    }
}
