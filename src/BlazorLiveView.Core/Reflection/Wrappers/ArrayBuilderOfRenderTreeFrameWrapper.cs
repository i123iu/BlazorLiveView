using Microsoft.AspNetCore.Components.RenderTree;
using System.Linq.Expressions;
using System.Reflection;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class ArrayBuilderOfRenderTreeFrameWrapper
    : WrapperBase
{
    private static readonly Type InnerType = Types.ArrayBuilderOfRenderTreeFrame;
    private static readonly MethodInfo _Append;
    private static readonly Func<object, ReadOnlySpan<RenderTreeFrame>, int> _AppendDelegate;

    static ArrayBuilderOfRenderTreeFrameWrapper()
    {
        _Append = InnerType.GetRequiredMethod(
            "Append",
            isPublic: false,
            parameters: [typeof(ReadOnlySpan<RenderTreeFrame>)]
        );
        _AppendDelegate = BuildAppendDelegate();
    }

    private static Func<object, ReadOnlySpan<RenderTreeFrame>, int> BuildAppendDelegate()
    {
        // The Append method in ArrayBuilder<RenderTreeFrame> has a parameter
        // of ReadOnlySpan<RenderTreeFrame>, which is a ref struct and cannot be
        // directly called via reflection (it would have to be cast to an object).

        var instanceParameter = Expression.Parameter(typeof(object), "instance");
        var sourceParameter = Expression.Parameter(typeof(ReadOnlySpan<RenderTreeFrame>), "source");
        return Expression.Lambda<Func<object, ReadOnlySpan<RenderTreeFrame>, int>>(
            Expression.Call(
                Expression.Convert(
                    instanceParameter,
                    InnerType
                ),
                _Append,
                sourceParameter
            ),
            instanceParameter,
            sourceParameter
        ).Compile();
    }

    public ArrayBuilderOfRenderTreeFrameWrapper(object inner)
        : base(inner)
    {
        Types.AssertIsInstanceOfType(InnerType, inner);
    }

    public void Append(ReadOnlySpan<RenderTreeFrame> source)
    {
        _AppendDelegate(Inner, source);
    }
}
