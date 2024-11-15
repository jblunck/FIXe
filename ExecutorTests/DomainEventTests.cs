using Executor.Domain;
using Xunit;

namespace ExecutorTests;

public class DomainEventTests
{
    public class ClassEvent(string argument)
    {
        public string Argument { get; } = argument;
    }

    [Fact]
    public void DomainEvent_ClassEvent()
    {
        // Given
        string result = string.Empty;

        void OnEvent(ClassEvent @event)
        {
            result = @event.Argument;
        }

        // When
        using (DomainEvent.Register<ClassEvent>(OnEvent))
        {
            DomainEvent.Raise(new ClassEvent(nameof(ClassEvent)));
        }
 
        // Then
        Assert.Equal(nameof(ClassEvent), result);
    }

    public readonly struct StructEvent(string argument)
    {
        public string Argument { get; } = argument;
    }

    [Fact]
    public void DomainEvent_StructEvent()
    {
        // Given
        string result = string.Empty;

        void OnEvent(StructEvent @event)
        {
            result = @event.Argument;
        }

        // When
        using (DomainEvent.Register<StructEvent>(OnEvent))
        {
            DomainEvent.Raise(new StructEvent(nameof(StructEvent)));
        }
 
        // Then
        Assert.Equal(nameof(StructEvent), result);
    }
}