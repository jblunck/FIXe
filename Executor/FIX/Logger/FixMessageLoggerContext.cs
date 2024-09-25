using System.Collections;
using System.Globalization;

namespace Executor.FIX.Logger;

public readonly struct FixMessageWithEventProperties : IReadOnlyList<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>
{
    public readonly string Message;
    public readonly string SessionName;
    public readonly string SessionLogon;

    readonly KeyValuePair<string,object> First;
    readonly KeyValuePair<string,object> Second;

    public FixMessageWithEventProperties(string message, string name, string logon)
    {
        Message = message;
        SessionName = name;
        SessionLogon = logon;

        First = new KeyValuePair<string, object>("session-name", SessionName);
        Second = new KeyValuePair<string, object>("session-logon", SessionLogon);
    }

    public KeyValuePair<string, object> this[int index] => index switch
    {
        0 => First,
        1 => Second,
        _ => throw new IndexOutOfRangeException(nameof(index))
    };

    public int Count => 2;

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        yield return this[0];
        yield return this[1];
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}