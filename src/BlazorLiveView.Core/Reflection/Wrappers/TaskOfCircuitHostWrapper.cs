using System.Reflection;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class TaskOfCircuitHostWrapper
    : WrapperBase
{
    private static readonly Type InnerType = Types.TaskOfCircuitHost;
    private static readonly MethodInfo _continueWith;

    static TaskOfCircuitHostWrapper()
    {
        foreach (var m in InnerType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
        {
            if (m.Name != "ContinueWith" || !m.IsGenericMethodDefinition)
                continue;

            var parameters = m.GetParameters();
            if (parameters.Length == 1)
            {
                var p = parameters[0].ParameterType;
                if (p.IsGenericType && p.GetGenericTypeDefinition() == typeof(Func<,>))
                {
                    _continueWith = m.MakeGenericMethod(Types.CircuitHost);
                    break;
                }
            }
        }

        if (_continueWith is null)
        {
            throw new Exception("Could not find ContinueWith function");
        }
    }

    public TaskOfCircuitHostWrapper(object inner)
        : base(inner)
    {
        Types.AssertIsInstanceOfType(InnerType, inner);
    }

    public TaskOfCircuitHostWrapper ContinueWith(Delegate continuationDelegate)
    {
        object task = _continueWith.Invoke(Inner, new[] { continuationDelegate })!;
        return new(task);
    }
}
