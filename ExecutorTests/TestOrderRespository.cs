using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Executor.Domain;
using Executor.Infrastructure;

namespace ExecutorTests;

public class TestOrderRepository : IOrderRepository
{
    internal ConcurrentDictionary<Guid, Order> _orders = new();

    public bool TryAdd(Order order)
    {
        return _orders.TryAdd(order.ID, order);
    }

    public void RemoveByOrderID(Guid id)
    {
        _orders.Remove(id, out _);
    }

    public bool FindByOrderID(Guid id, [MaybeNullWhen(false)] out Order order)
    {
        return _orders.TryGetValue(id, out order);
    }
}