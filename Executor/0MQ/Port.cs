using System.Collections.Concurrent;

namespace Executor.ZeroMQ;

// The ZeroMQ dealer sends new order requests to the matching engine

public interface SecurityReferenceDataRepository
{
    public Guid FindBySecurityName(string name);
}


public interface IPortState
{
    // we have one sequence # per instrument
    public ulong SequenceNumber { get; }

    // we only support one outstanding request per instrument
    public Guid OpenRequest { get; }

    public bool TryAddRequest(Guid id, string request);

    // we might have multiple open orders per port/dealer though
    public IEnumerator<Guid> GetAsks { get; }
    public IEnumerator<Guid> GetBids { get; }
}