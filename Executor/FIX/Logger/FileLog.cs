using System.Globalization;
using QuickFix.Logger;

namespace Executor.FIX.Logger;

public class FileLog : ILog
{
    private readonly ILogger _logger;
    private readonly ILogger _msgLogger;
    private string SessionName { get; }
    public string? SessionLogon { get; set; } = null;

    readonly Action<ILogger, DateTime, string, Exception?> _logOnEvent =
        LoggerMessage.Define<DateTime,string>(
            LogLevel.Information,
            new EventId(1, nameof(OnEvent)),
            "{DateTime}: {Event}",
            new LogDefineOptions { SkipEnabledCheck = true }
        );

    readonly Action<ILogger, string> _logOnIncoming; // = LoggerMessage.Define<DateTime,string>(
        // LogLevel.Information,
        // new EventId(2, nameof(OnIncoming)),
        // "{DateTime}: {Message}");

    readonly Action<ILogger, string> _logOnOutgoing; // = LoggerMessage.Define<DateTime,string>(
        // LogLevel.Information,
        // new EventId(3, nameof(OnOutgoing)),
        // "{DateTime}: {Message}");

    private static Action<ILogger,string> Define(LogLevel logLevel, EventId eventId, FileLog ctx)
    {
        return Log;

        void Log(ILogger logger, string arg1)
        {
            logger.Log(logLevel, eventId, new FixMessageWithEventProperties(arg1, ctx.SessionName, ctx.SessionLogon ?? DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture)), exception: null, formatter: static(s, _) => s.Message);
        }
    }

    public FileLog(ILogger logger, ILogger msgLogger, string prefix)
    {
        _logger = logger;
        _msgLogger = msgLogger;
        SessionName = prefix;

        _logOnIncoming = Define(LogLevel.Information, new EventId(2, nameof(OnIncoming)), this);
        _logOnOutgoing = Define(LogLevel.Information, new EventId(3, nameof(OnOutgoing)), this);
    }

    public void Clear()
    {
        // throw new NotImplementedException();
    }

    public void Dispose()
    {
        // throw new NotImplementedException();
    }

    public void OnEvent(string s)
    {
        _logOnEvent(_logger, DateTime.UtcNow, s, default!);
    }

    public void OnIncoming(string msg)
    {
        _logOnIncoming(_msgLogger, msg);
    }

    public void OnOutgoing(string msg)
    {
        _logOnOutgoing(_msgLogger, msg);
    }

    public void SetSessionLogon(DateTime dateTime)
    {
        SessionLogon = dateTime.ToString("yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture);
    }
}