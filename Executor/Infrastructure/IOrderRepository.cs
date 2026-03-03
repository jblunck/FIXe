using System.Diagnostics.CodeAnalysis;
using Executor.Domain;

namespace Executor.Infrastructure;

public interface IOrderRepository
{
    bool TryAdd(Order order);
    void RemoveByOrderID(Guid id);

    bool FindByOrderID(Guid id, [MaybeNullWhen(false)] out Order order);
    // ConfirmedOrders
    // InflightOrders
}
