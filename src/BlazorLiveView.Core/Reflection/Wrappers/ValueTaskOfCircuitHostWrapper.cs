using System.Reflection;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class ValueTaskOfCircuitHostWrapper
    : WrapperBase
{
    private static readonly Type WrappedType = Types.ValueTaskOfCircuitHost;
    private static readonly MethodInfo _AsTask;

    static ValueTaskOfCircuitHostWrapper()
    {
        _AsTask = WrappedType.GetRequiredMethod("AsTask", isPublic: true);
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
