using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Executor.Domain;
using Executor.Services;
using Xunit;

namespace ExecutorTests;

public class UnitTest1
{
    [Fact]
    public async Task Test1()
    {
        PortOrderProcessor processor = new(new TestOrderRepository());
        List<Guid> executionReports = new();
        processor.OnOrderExecuted += (execReport) => executionReports.Add(execReport.OrderID);
        Assert.True(processor.NewOrderSingle(new Order() { ID = Guid.Empty, Instrument = Guid.Empty }));
        Assert.False(processor.NewOrderSingle(new Order() { ID = Guid.Empty, Instrument = Guid.Empty }));
        await processor.StartAsync(CancellationToken.None);
        await Task.Delay(1000);
        Assert.NotEmpty(executionReports);
        Assert.Equal(Guid.Empty, executionReports[0]);
        await processor.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task TwoOrdersSameInstrument()
    {
        PortOrderProcessor processor = new(new TestOrderRepository());
        List<Guid> executionReports = new();
        processor.OnOrderExecuted += (execReport) => executionReports.Add(execReport.OrderID);
        Assert.True(processor.NewOrderSingle(new Order() { ID = Guid.NewGuid(), Instrument = Guid.Empty }));
        Assert.False(processor.NewOrderSingle(new Order() { ID = Guid.NewGuid(), Instrument = Guid.Empty }));
        await processor.StartAsync(CancellationToken.None);
        await Task.Delay(1000);
        Assert.Single(executionReports);
        await processor.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task TwoOrdersDifferentInstrument()
    {
        PortOrderProcessor processor = new(new TestOrderRepository());
        List<Guid> executionReports = new();
        processor.OnOrderExecuted += (execReport) => executionReports.Add(execReport.OrderID);
        Assert.True(processor.NewOrderSingle(new Order() { ID = Guid.NewGuid(), Instrument = Guid.NewGuid() }));
        Assert.True(processor.NewOrderSingle(new Order() { ID = Guid.NewGuid(), Instrument = Guid.NewGuid() }));
        await processor.StartAsync(CancellationToken.None);
        await Task.Delay(1000);
        Assert.Equal(2, executionReports.Count);
        await processor.StopAsync(CancellationToken.None);
    }
}