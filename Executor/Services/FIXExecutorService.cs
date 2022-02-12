using QuickFix;

namespace Executor.Services;

public class FIXExecutorService : IHostedService, IDisposable
{
    IApplication _application;
    ThreadedSocketAcceptor _acceptor;
    SessionSettings _settings;
    IMessageStoreFactory _storeFactory;

    private readonly ILogger<FIXExecutorService> _logger;

    public FIXExecutorService(ILogger<FIXExecutorService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _application = new FIX.Executor();
        _settings =  new SessionSettings("quickfix.cfg");
        _storeFactory   = new FileStoreFactory(_settings);
        _acceptor = new ThreadedSocketAcceptor(_application, _storeFactory, _settings);
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service running.");
        _acceptor.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service is stopping.");

        _acceptor.Stop();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }
}