﻿using System.Reflection;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class TaskOfCircuitHostWrapper
    : WrapperBase
{
    private static readonly Type WrappedType = Types.TaskOfCircuitHost;
    private static readonly MethodInfo _continueWith;

    static TaskOfCircuitHostWrapper()
    {
        foreach (var m in WrappedType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
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

    public TaskOfCircuitHostWrapper(object wrapped) : base(wrapped)
    {
        Types.AssertIsInstanceOfType(WrappedType, wrapped);
    }

    public TaskOfCircuitHostWrapper ContinueWith(Delegate continuationDelegate)
    {
        object task = _continueWith.Invoke(Inner, new[] { continuationDelegate })!;
        return new(task);
    }
}
