namespace Executor.Domain;

public enum Side
{
    NONE = 0,
    BUY = 1,
    SELL = 2
}

public class Order
{
    public Guid ID { get; init; }
    public Guid Instrument { get; init; }
    public Side Side { get; init; }
    public uint OrderQty { get; init; }
    public decimal Price { get; init; }
}
