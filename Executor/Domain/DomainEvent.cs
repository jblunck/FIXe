namespace Executor.Domain;

public static class DomainEvent
{
    [ThreadStatic]
    private static List<Delegate>? _actions;
    private static List<Delegate> Actions
    {
        get => _actions ??= new(4);
    }

    public static DomainEventRegistrationDisposer Register<T>(Action<T> callback)
    {
        Actions.Add(callback);
        return new DomainEventRegistrationDisposer(() => Actions.Remove(callback));
    }

    public static void Raise<T>(T eventArg)
    {
        foreach (Delegate action in Actions)
        {
            Action<T>? typedAction = action as Action<T>;
            typedAction?.Invoke(eventArg);
        }
    }

    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/ref-struct#the-disposable-pattern
    public readonly ref struct DomainEventRegistrationDisposer(Action callback)
    {
        private readonly Action _callback = callback;

        public void Dispose() => _callback();
    }
}
