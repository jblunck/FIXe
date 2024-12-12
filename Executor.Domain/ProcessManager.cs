using System.ComponentModel.Design;
using System.Net;
using System.Security;
using System.Security.Principal;

namespace Executor.Domain;

public class RestoreOrderProcess
{
    public Guid Id { get; }

    private CancellationTokenSource _tokenSource;
    private TaskCompletionSource _completionSource;
    public Task Task { get; }

    private enum RestoreState {
        NOT_STARTED,
        STARTED_LOCAL,
        STARTED_REMOTE,
    };

    private RestoreState _status = RestoreState.NOT_STARTED;

    public RestoreOrderProcess()
    {
        Id = Guid.NewGuid();
        _tokenSource = new();
        _completionSource = new();
        _tokenSource.Token.Register(() => _completionSource.TrySetCanceled());
        Task = _completionSource.Task;

        _activeTasks = new();
    }

    private TimeSpan _timeout = TimeSpan.FromSeconds(60);

    private List<Task> _activeTasks;

    public void RecoverLocalOrders()
    {
        if (_status != RestoreState.NOT_STARTED)
        {
            throw new InvalidOperationException(string.Format("Can't start order recovery in status '{0}'", _status.ToString()));
        }
        _status = RestoreState.STARTED_LOCAL;

        _activeTasks.Add(Task.Run(async () => {
            using var stoppingTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_tokenSource.Token);
            stoppingTokenSource.CancelAfter(_timeout);

            foreach(var file in Directory.EnumerateFiles("orders"))
            {
                // throw is cancellation requested

                var content = await File.ReadAllLinesAsync(file, stoppingTokenSource.Token);

            }

            // transition into next step
        }, _tokenSource.Token));
    }

    public void RecoverRemoteOrders()
    {
        if (_status != RestoreState.STARTED_LOCAL)
        {
            throw new InvalidOperationException(string.Format("Can't start order recovery in status '{0}'", _status.ToString()));
        }
        _status = RestoreState.STARTED_REMOTE;


    }
}

public interface IConnector
{
    void Subscribe(string topic);
}

public interface IEvent { }

public class LocalOrdersProcessedEvent : IEvent { }

public class OrderSnapshotReceivedEvent : IEvent { }

public class PrivateTradeSnapshotReceivedEvent : IEvent
{
    public PrivateTradeSnapshotReceivedEvent()
    {
        Trades = new List<string>();
    }

    public IReadOnlyCollection<string> Trades { get; }
}

public class RecoverOrdersProcess
{
    IConnector connector;

    public RecoverOrdersProcess(IConnector connector)
    {
        this.connector = connector;
    }

    public void Handle(LocalOrdersProcessedEvent @event)
    {
        // subscribe to Order topic
        connector.Subscribe("orders");
    }

    public void Handle(OrderSnapshotReceivedEvent @event)
    {
        // subscribe to PrivateTrade topic
        connector.Subscribe("privateTrade");
    }

    public void Handle(PrivateTradeSnapshotReceivedEvent @event)
    {
        // check if we might need to recover additionally from REST api
        if (@event.Trades.Count > 2000)
        {

        }

        // raise Event completed
    }
}


public class ProcessManager
{
    private List<RestoreOrderProcess> restoreOrderProcesses;

    public ProcessManager()
    {
        restoreOrderProcesses = new();
    }
}
