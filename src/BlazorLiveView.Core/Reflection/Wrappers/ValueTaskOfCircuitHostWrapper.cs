using System.Reflection;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class ValueTaskOfCircuitHostWrapper
    : WrapperBase
{
    private static readonly Type InnerType = Types.ValueTaskOfCircuitHost;
    private static readonly ConstructorInfo _ConstructorFromTask = InnerType
        .GetConstructor(new[] { Types.TaskOfCircuitHost })!;
    private static readonly MethodInfo _AsTask;

    static ValueTaskOfCircuitHostWrapper()
    {
        _AsTask = InnerType.GetRequiredMethod("AsTask", isPublic: true);
    }

    public static ValueTaskOfCircuitHostWrapper Constructor(TaskOfCircuitHostWrapper task)
    {
        object valueTask = _ConstructorFromTask.Invoke(new[] { task.Inner })!;
        return new(valueTask);
    }

    public ValueTaskOfCircuitHostWrapper(object inner)
        : base(inner)
    {
        Types.AssertIsInstanceOfType(InnerType, inner);
    }

    public TaskOfCircuitHostWrapper AsTask()
    {
        object task = _AsTask.Invoke(Inner, Array.Empty<object>())
            ?? throw new Exception("AsTask returned null");
        return new(task);
    }
}
