using System.Reflection;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class ValueTaskOfCircuitHostWrapper
    : WrapperBase
{
    private static readonly Type WrappedType = Types.ValueTaskOfCircuitHost;
    private static readonly ConstructorInfo _ConstructorFromTask = WrappedType
        .GetConstructor(new[] { Types.TaskOfCircuitHost })!;
    private static readonly MethodInfo _AsTask;

    static ValueTaskOfCircuitHostWrapper()
    {
        _AsTask = WrappedType.GetRequiredMethod("AsTask", isPublic: true);
    }

    public static ValueTaskOfCircuitHostWrapper Constructor(TaskOfCircuitHostWrapper task)
    {
        object valueTask = _ConstructorFromTask.Invoke(new[] { task.Inner })!;
        return new(valueTask);
    }

    public ValueTaskOfCircuitHostWrapper(object wrapped) : base(wrapped)
    {
        Types.AssertIsInstanceOfType(WrappedType, wrapped);
    }

    public TaskOfCircuitHostWrapper AsTask()
    {
        object task = _AsTask.Invoke(Inner, Array.Empty<object>())!;
        return new(task);
    }
}
