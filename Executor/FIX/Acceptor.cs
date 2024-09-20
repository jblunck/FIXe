using System.Collections.Concurrent;
using QuickFix;

namespace Executor.FIX;

public class Acceptor : IApplication, IApplicationExt
{
    private readonly ConcurrentDictionary<SessionID, MessageCracker> _messageCrackers = new();

    public void OnCreate(SessionID sessionID)
    {
        var cracker = new Executor();
        if (!_messageCrackers.TryAdd(sessionID, cracker))
            throw new ApplicationException("Failed to add MessageCracker");
    }

    // called after we received and sent a logon
    public void OnLogon(SessionID sessionID)
    {
        // we might have sent a ResendRequest already
        // refresh FIX session state based on received messages
        // refresh NPS session state based on received messages
        // 1. our NPS state might lag behind so we shouldn't send duplicates (maintain a deduplication table with TTL)
        // 2. the FIX session state might lag behind the NPS state, so lets replay missed messages

        // eventually send News message including the ID of our pod
    }

    public void OnLogout(SessionID sessionID)
    {
    }

    // everything we receive before verification
    public void FromEarlyIntercept(Message message, SessionID sessionID)
    {
    }

    // messages we receive after successful verification
    // messages that are rejected and generate a reject are interceptable in ToAdmin()
    public void FromAdmin(Message message, SessionID sessionID)
    {
        // check the PASSWD and throw new RejectLogon("bad password");
        Console.WriteLine("IN(admin): " + message.ToString().Replace(Message.SOH, '|'));
    }

    public void FromApp(Message message, SessionID sessionID)
    {
        Console.WriteLine("IN:  " + message.ToString().Replace(Message.SOH, '|'));

        try
        {
            if (!_messageCrackers.TryGetValue(sessionID, out var cracker))
            {
                throw new SessionNotFound(sessionID, "No MessageCracker found");
            }
            cracker.Crack(message, sessionID);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception:" + ex.Message);
            Session? session = Session.LookupSession(sessionID) ?? throw new AggregateException(new SessionNotFound(sessionID), ex);
            session.Disconnect(ex.Message);
        }
    }

    // everything admin message before we actually send it
    public void ToAdmin(Message message, SessionID sessionID)
    {
        Console.WriteLine("OUT(admin): " + message.ToString().Replace(Message.SOH, '|'));
    }

    // every business message before we actually send it
    public void ToApp(Message message, SessionID sessionID)
    {
        Console.WriteLine("OUT: " + message.ToString().Replace(Message.SOH, '|'));

        // to prevent messages from being sent throw new DoNotSend();
    }
}
