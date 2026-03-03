namespace Executor.Domain;

public class OrderExecutionReport
{
    public Guid ID { get; init; }
    public Guid Instrument { get; init; }
    public Guid OrderID { get; init; }
    public Side Side { get; init; }
    public uint OrderQty { get; init; }
    public decimal Price { get; init; }
    public uint CumQty { get; init; }
}