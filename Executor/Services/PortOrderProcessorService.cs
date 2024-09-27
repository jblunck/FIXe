using System.Collections.Concurrent;
using Executor.Domain;
using Executor.Infrastructure;

namespace Executor.Services;

public class PortOrderProcessor : BackgroundService, IOrderProcessorService
{
    private readonly ConcurrentDictionary<Guid, Order> _requestsByInstrument;
    private readonly IOrderRepository _repository;
    private readonly BlockingCollection<OrderExecutionReport> _executions;
    public event OrderExecuted OnOrderExecuted;

    public PortOrderProcessor(IOrderRepository repository)
    {
        _requestsByInstrument = new();
        _executions = new();
        _repository = repository;
        OnOrderExecuted += OrderExecuted;
    }

    public bool NewOrderSingle(Order order)
    {
        if (!_repository.TryAdd(order))
            return false;

        if (!_requestsByInstrument.TryAdd(order.Instrument, order))
        {
            _repository.RemoveByOrderID(order.ID);
            return false;
        }

        OrderExecutionReport executionReport = new()
        {
            ID = Guid.NewGuid(),
            Instrument = order.Instrument,
            OrderID = order.ID,
        };
        _executions.Add(executionReport);
        return true;
    }

    private void OrderExecuted(OrderExecutionReport execution)
    {
        // add order to repository
        if (!_repository.FindByOrderID(execution.OrderID, out var order))
        {
            Console.WriteLine("execution report for unknown order");
            return;
        }

        // _repository.Add(order);
        _requestsByInstrument.Remove(execution.Instrument, out _);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() =>
        {
            try
            {
                foreach (var executionReport in _executions.GetConsumingEnumerable(stoppingToken))
                {
                    OnOrderExecuted?.Invoke(executionReport);
                }
            }
            catch (OperationCanceledException)
            {
                if (stoppingToken.IsCancellationRequested)
                    return;
                throw;
            }
        }, stoppingToken);
    }
}