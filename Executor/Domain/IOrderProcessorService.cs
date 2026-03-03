namespace Executor.Domain;

public delegate void OrderExecuted(OrderExecutionReport execution);

public interface IOrderProcessorService
{
    bool NewOrderSingle(Order order);

    event OrderExecuted OnOrderExecuted;
}
