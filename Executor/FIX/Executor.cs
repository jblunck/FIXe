using Executor.Domain;
using Executor.Infrastructure;
using Executor.Services;
using QuickFix;
using QuickFix.Fields;

namespace Executor.FIX;

public class Executor : MessageCracker
{
    static readonly decimal DEFAULT_MARKET_PRICE = 10;

    private readonly SessionID _sessionID;
    private readonly IOrderProcessorService _orderProcessor;

    int orderID = 0;
    int execID = 0;

    public Executor(SessionID sessionID, IOrderProcessorService orderProcessor)
    {
        _sessionID = sessionID;
        _orderProcessor = orderProcessor;
        _orderProcessor.OnOrderExecuted += OrderExecuted;
    }

    private string GenOrderID() { return (++orderID).ToString(); }
    private string GenExecID() { return (++execID).ToString(); }

    #region MessageCracker overloads

    private void OrderExecuted(OrderExecutionReport executionReport)
    {
        QuickFix.FIX44.ExecutionReport exReport = new
        (
            new OrderID(GenOrderID()),
            new ExecID(GenExecID()),
            // new ExecTransType(ExecTransType.NEW),
            new ExecType(ExecType.FILL),
            new OrdStatus(OrdStatus.FILLED),
            new Symbol(executionReport.Instrument.ToString()),
            executionReport.Side.ToSide(),
            new LeavesQty(0),
            new CumQty(executionReport.OrderQty),
            new AvgPx(executionReport.Price));

        exReport.Set(new ClOrdID(executionReport.OrderID.ToString()));
        exReport.Set(new OrderQty(executionReport.OrderQty));
        exReport.Set(new LastQty(executionReport.OrderQty));
        exReport.Set(new LastPx(executionReport.Price));

        // if (n.IsSetAccount())
        //     exReport.SetField(n.Account);

        try
        {
            Session.SendToTarget(exReport, _sessionID);
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

    public void OnMessage(QuickFix.FIX44.NewOrderSingle n, SessionID s)
    {
        Symbol symbol = n.Symbol;
        QuickFix.Fields.Side side = n.Side;
        OrdType ordType = n.OrdType;
        OrderQty orderQty = n.OrderQty;
        ClOrdID clOrdID = n.ClOrdID;
        Price price = new (DEFAULT_MARKET_PRICE);

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

        Guid instrument = Guid.NewGuid();

        if (!_orderProcessor.NewOrderSingle(new Domain.Order()
        { 
            ID = Guid.NewGuid(),
            Instrument = instrument,
            Side = side.ToSide(),
            OrderQty = (uint)orderQty.Obj,
            Price = price.Obj
        }))
        {
            // send reject
            QuickFix.FIX44.ExecutionReport exReport = new
            (
                new OrderID(GenOrderID()),
                new ExecID(GenExecID()),
                new ExecType(ExecType.REJECTED),
                new OrdStatus(OrdStatus.REJECTED),
                n.Symbol,
                n.Side,
                new LeavesQty(0),
                new CumQty(0),
                new AvgPx(0)
            );

            exReport.Set(clOrdID);
            exReport.Set(orderQty);
            exReport.Set(new LastQty(0));

            try
            {
                Session.SendToTarget(exReport, _sessionID);
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
