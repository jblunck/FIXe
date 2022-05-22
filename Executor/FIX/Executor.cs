using QuickFix;
using QuickFix.Fields;

namespace Executor.FIX;

public class Executor : QuickFix.MessageCracker, QuickFix.IApplication
{
    static readonly decimal DEFAULT_MARKET_PRICE = 10;

    int orderID = 0;
    int execID = 0;

    private string GenOrderID() { return (++orderID).ToString(); }
    private string GenExecID() { return (++execID).ToString(); }

    #region QuickFix.Application Methods

    public void FromApp(Message message, SessionID sessionID)
    {
        Console.WriteLine("IN:  " + message);
        Crack(message, sessionID);
    }

    public void ToApp(Message message, SessionID sessionID)
    {
        Console.WriteLine("OUT: " + message);
    }

    public void FromAdmin(Message message, SessionID sessionID) { }
    public void OnCreate(SessionID sessionID) { }
    public void OnLogout(SessionID sessionID) { }
    public void OnLogon(SessionID sessionID) { }
    public void ToAdmin(Message message, SessionID sessionID) { }
    #endregion

    #region MessageCracker overloads

    public void OnMessage(QuickFix.FIX42.NewOrderSingle n, SessionID s)
    {
        Symbol symbol = n.Symbol;
        Side side = n.Side;
        OrdType ordType = n.OrdType;
        OrderQty orderQty = n.OrderQty;
        ClOrdID clOrdID = n.ClOrdID;
        Price price = new Price(DEFAULT_MARKET_PRICE);

        switch (ordType.getValue())
        {
            case OrdType.LIMIT:
                price = n.Price;
                if (price.Obj == 0)
                    throw new IncorrectTagValue(price.Tag);
                break;
            case OrdType.MARKET: break;
            default: throw new IncorrectTagValue(ordType.Tag);
        }

        QuickFix.FIX42.ExecutionReport exReport = new QuickFix.FIX42.ExecutionReport(
            new OrderID(GenOrderID()),
            new ExecID(GenExecID()),
            new ExecTransType(ExecTransType.NEW),
            new ExecType(ExecType.FILL),
            new OrdStatus(OrdStatus.FILLED),
            symbol,
            side,
            new LeavesQty(0),
            new CumQty(orderQty.getValue()),
            new AvgPx(price.getValue()));

        exReport.Set(clOrdID);
        exReport.Set(orderQty);
        exReport.Set(new LastShares(orderQty.getValue()));
        exReport.Set(new LastPx(price.getValue()));

        if (n.IsSetAccount())
            exReport.SetField(n.Account);

        try
        {
            Session.SendToTarget(exReport, s);
        }
        catch (SessionNotFound ex)
        {
            Console.WriteLine("==session not found exception!==");
            Console.WriteLine(ex.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    public void OnMessage(QuickFix.FIX42.OrderCancelRequest msg, SessionID s)
    {
        string orderid = (msg.IsSetOrderID()) ? msg.OrderID.Obj : "unknown orderID";
        QuickFix.FIX42.OrderCancelReject ocj = new QuickFix.FIX42.OrderCancelReject(
            new OrderID(orderid), msg.ClOrdID, msg.OrigClOrdID, new OrdStatus(OrdStatus.REJECTED), new CxlRejResponseTo(CxlRejResponseTo.ORDER_CANCEL_REQUEST));
        ocj.CxlRejReason = new CxlRejReason(CxlRejReason.UNKNOWN_ORDER);
        ocj.Text = new Text("Executor does not support order cancels");

        try
        {
            Session.SendToTarget(ocj, s);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    public void OnMessage(QuickFix.FIX42.OrderCancelReplaceRequest msg, SessionID s)
    {
        string orderid = (msg.IsSetOrderID()) ? msg.OrderID.Obj : "unknown orderID";
        QuickFix.FIX42.OrderCancelReject ocj = new QuickFix.FIX42.OrderCancelReject(
            new OrderID(orderid), msg.ClOrdID, msg.OrigClOrdID, new OrdStatus(OrdStatus.REJECTED), new CxlRejResponseTo(CxlRejResponseTo.ORDER_CANCEL_REPLACE_REQUEST));
        ocj.CxlRejReason = new CxlRejReason(CxlRejReason.UNKNOWN_ORDER);
        ocj.Text = new Text("Executor does not support order cancel/replaces");

        try
        {
            Session.SendToTarget(ocj, s);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    public void OnMessage(QuickFix.FIX42.News n, SessionID s) { }

    // FIX40-41 don't have rejects
    public void OnMessage(QuickFix.FIX42.BusinessMessageReject n, SessionID s) { }

    #endregion //MessageCracker overloads
}
