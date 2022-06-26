using Executor.Services;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;
var logger = LoggerFactory.Create(config =>
{
    if (!env.IsDevelopment() && System.Console.IsInputRedirected)
        config.AddJsonConsole();
    else
        config.AddConsole();
}).CreateLogger(nameof(Program));

logger.LogInformation("Starting {ExecutingAssemblyName} ({EnvironmentName})", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, env.EnvironmentName);

builder.Services.AddFixGatewayShell();
builder.Host.UseFixGatewayShell();

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

app.UseOpenTelemetryPrometheusScrapingEndpoint();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
