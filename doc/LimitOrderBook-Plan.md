# Limit Order Book — C# Implementation Plan

## Overview

The Limit Order Book (LOB) tracks two sides: **Bids** (buy orders, highest price first) and **Asks** (sell orders, lowest price first). It requires O(log n) price-level operations and O(1) order lookup by ID.

---

## Data Structures

### `Order` (record)

| Field       | Type                | Description        |
| ----------- | ------------------- | ------------------ |
| `OrderId`   | `string`            | Unique identifier  |
| `Side`      | `enum { Bid, Ask }` | Buy or sell side   |
| `Price`     | `decimal`           | Limit price        |
| `Qty`       | `decimal`           | Order quantity     |
| `Timestamp` | `DateTime`          | Time of submission |

### `PriceLevel`

Holds a doubly-linked list of orders at a given price, plus a running `TotalQty`. Adding and removing orders is O(1) via the hashmap's node reference.

### `LimitOrderBook`

| Field       | Type                                        | Purpose                                    |
| ----------- | ------------------------------------------- | ------------------------------------------ |
| `_orderMap` | `Dictionary<string, LinkedListNode<Order>>` | O(1) lookup by order ID                    |
| `_bids`     | `SortedDictionary<decimal, PriceLevel>`     | Descending comparer — best bid = max price |
| `_asks`     | `SortedDictionary<decimal, PriceLevel>`     | Ascending comparer — best ask = min price  |

> **Note on skip-lists:** .NET's `SortedDictionary<K,V>` is a red-black tree delivering O(log n) operations — the same asymptotic guarantees as a skip-list. A true skip-list would require a custom implementation or a NuGet package. The plan uses `SortedDictionary` but isolates it behind an interface so it can be swapped out later.

---

## Public API

| Method | Signature                                                           | Description                                       |
| ------ | ------------------------------------------------------------------- | ------------------------------------------------- |
| Add    | `void AddOrder(Order order)`                                        | Insert a new order                                |
| Cancel | `bool CancelOrder(string orderId)`                                  | Remove an order by ID                             |
| Modify | `bool ModifyOrder(string orderId, decimal newQty)`                  | Change quantity; cancel + re-add if price changes |
| Query  | `decimal? BestBid()`                                                | Top-of-book bid price                             |
| Query  | `decimal? BestAsk()`                                                | Top-of-book ask price                             |
| Query  | `IReadOnlyList<PriceLevelSnapshot> GetDepth(Side side, int levels)` | N-level depth snapshot                            |
| Query  | `Order? GetOrder(string orderId)`                                   | Single order lookup                               |

---

## Project Structure

```shell
LimitOrderBookLib/
├── Models/
│   ├── Order.cs
│   ├── Side.cs
│   └── PriceLevelSnapshot.cs
├── Core/
│   ├── PriceLevel.cs
│   └── LimitOrderBook.cs
└── Comparers/
    └── DescendingDecimalComparer.cs

LimitOrderBook.Tests/
├── AddOrderTests.cs
├── CancelOrderTests.cs
├── ModifyOrderTests.cs
├── BestBidAskTests.cs
├── DepthTests.cs
└── ConcurrencyTests.cs   (optional)
```

---

## Key Implementation Details

### Adding an Order

```csharp
public void AddOrder(Order order)
{
    if (_orderMap.ContainsKey(order.OrderId))
        throw new DuplicateOrderException(order.OrderId);

    var book = order.Side == Side.Bid ? _bids : _asks;

    if (!book.TryGetValue(order.Price, out var level))
    {
        level = new PriceLevel(order.Price);
        book[order.Price] = level;
    }

    var node = level.AddOrder(order);   // O(1) — append to linked list
    _orderMap[order.OrderId] = node;    // O(1) — hashmap insert
}
```

### Cancelling an Order

```csharp
public bool CancelOrder(string orderId)
{
    if (!_orderMap.TryGetValue(orderId, out var node)) return false;

    var order = node.Value;
    var book  = order.Side == Side.Bid ? _bids : _asks;
    var level = book[order.Price];

    level.RemoveOrder(node);             // O(1) — linked list node removal
    if (level.TotalQty == 0)
        book.Remove(order.Price);        // O(log n) — clean up empty level

    _orderMap.Remove(orderId);
    return true;
}
```

### Price Level

```csharp
internal class PriceLevel
{
    private readonly LinkedList<Order> _orders = new();
    public decimal Price    { get; }
    public decimal TotalQty { get; private set; }

    internal LinkedListNode<Order> AddOrder(Order o)
    {
        TotalQty += o.Qty;
        return _orders.AddLast(o);   // FIFO — preserves price-time priority
    }

    internal void RemoveOrder(LinkedListNode<Order> node)
    {
        TotalQty -= node.Value.Qty;
        _orders.Remove(node);
    }
}
```

---

## Unit Test Plan

### `AddOrderTests`

- Add a single bid/ask → order appears in book
- Add multiple orders at the same price → `TotalQty` aggregates correctly
- Add orders at different price levels → depth is correct
- Duplicate order ID → throws `DuplicateOrderException`

### `CancelOrderTests`

- Cancel an existing order → removed from `_orderMap` and price level qty decreases
- Cancel the last order at a price level → level is removed from the book
- Cancel a non-existent ID → returns `false`, no exception thrown

### `ModifyOrderTests`

- Reduce qty → level qty decreases; order stays at same FIFO position
- Increase qty → level qty increases
- Modify qty to zero → equivalent to cancel
- Modify a non-existent order → returns `false`

### `BestBidAskTests`

- Empty book → `BestBid()` / `BestAsk()` return `null`
- Single order → best bid/ask equals that price
- Multiple price levels → best bid is the **highest** price; best ask is the **lowest**
- After cancelling the best level → best updates to the next level

### `DepthTests`

- `GetDepth(Bid, 3)` returns the top 3 bid levels in descending price order
- `GetDepth(Ask, 5)` with only 2 levels present → returns 2 levels, not 5
- Snapshot is immutable — mutations after the call do not affect it

### `ConcurrencyTests` *(if thread-safety is in scope)*

- Concurrent adds do not corrupt `TotalQty`
- Concurrent add + cancel do not deadlock

---

## Complexity Summary

| Operation             | Time      | Notes                                        |
| --------------------- | --------- | -------------------------------------------- |
| `AddOrder`            | O(log n)  | Dominated by `SortedDictionary` insert       |
| `CancelOrder`         | O(log n)  | Dominated by `SortedDictionary` remove       |
| `GetOrder`            | O(1)      | Direct hashmap lookup                        |
| `BestBid` / `BestAsk` | O(log n)* | Can be reduced to O(1) by caching best price |
| `GetDepth(k)`         | O(k)      | Iterate top k levels                         |

*`SortedDictionary` Min/Max traversal is O(log n); caching the best price key reduces this to O(1) at the cost of slightly more complex bookkeeping on add/cancel.*
