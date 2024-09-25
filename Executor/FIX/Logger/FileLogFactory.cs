using NLog.Extensions.Logging;
using QuickFix;
using QuickFix.Logger;

namespace Executor.FIX.Logger;

// IMessageStore and ILog are doing similar things so we could
// - generate a message repository (AzureBlobStorage) that
//   - that persist messages we send (both IMessageStore and ILog)
//   - that persist messages we receive (only ILog)
// - maintain a cache on top of the repository
// - let the NonSessionLog and the Event log write to an ILogger for that session

public class FileLogFactory : ILogFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public FileLogFactory()
    {
        _loggerFactory = LoggerFactory.Create((loggingBuilder) => {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddNLog();
        });
    }

    public ILog Create(SessionID sessionId)
    {
        var prefix = QuickFix.Logger.FileLog.Prefix(sessionId);
        return new FileLog(_loggerFactory.CreateLogger("QuickFix.Events"), _loggerFactory.CreateLogger("QuickFix.Messages"), prefix);
    }

    public ILog CreateNonSessionLog()
    {
        return new NullLog();
    }
}