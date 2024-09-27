namespace Executor.Domain;

public class OrderExecutionReport
{
    public Guid ID { get; init; }
    public Guid Instrument { get; init; }
    public Guid OrderID { get; init; }
}