using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;

namespace Executor.Services;

public class FIXAcceptorService : IHostedService, IDisposable
{
    IApplication _application;
    ThreadedSocketAcceptor _acceptor;
    SessionSettings _settings;
    private readonly ILogFactory _logFactory;
    IMessageStoreFactory _storeFactory;
    private readonly IMessageFactory _messageFactory;

    private readonly ILogger<FIXAcceptorService> _logger;

    public FIXAcceptorService(ILogger<FIXAcceptorService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _application = new FIX.Acceptor();
        _settings =  new SessionSettings("quickfix.cfg");
        _storeFactory   = new FileStoreFactory(_settings);
        _logFactory = new Executor.FIX.Logger.FileLogFactory();
        _messageFactory = new DefaultMessageFactory();
        _acceptor = new ThreadedSocketAcceptor(_application, _storeFactory, _settings, _logFactory, _messageFactory);
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FIX Acceptor running.");
        _acceptor.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FIX Acceptor is stopping.");

        _acceptor.Stop();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }
}