using Executor.Domain;

namespace Executor.FIX;

public static class DomainExtensions
{
    public static QuickFix.Fields.Side ToSide(this Side side)
    {
        return side switch
        {
            Side.BUY => new QuickFix.Fields.Side(QuickFix.Fields.Side.BUY),
            Side.SELL => new QuickFix.Fields.Side(QuickFix.Fields.Side.SELL),
            _ => new QuickFix.Fields.Side(QuickFix.Fields.Side.UNDISCLOSED),
        };
    }

    public static Side ToSide(this QuickFix.Fields.Side side)
    {
        return side.Obj switch
        {
            QuickFix.Fields.Side.BUY => Side.BUY,
            QuickFix.Fields.Side.SELL => Side.SELL,
            _ => Side.NONE,
        };
    }

}