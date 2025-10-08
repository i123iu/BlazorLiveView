using Microsoft.AspNetCore.Components.RenderTree;
using System.Linq.Expressions;
using System.Reflection;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class ArrayBuilderOfRenderTreeFrameWrapper
    : WrapperBase
{
    private static readonly Type WrappedType = Types.RenderTreeFrameArrayBuilder.BaseType!;
    private static readonly MethodInfo _Append;

    static ArrayBuilderOfRenderTreeFrameWrapper()
    {
        _Append = WrappedType.GetRequiredMethod(
            "Append",
            isPublic: false,
            parameters: [typeof(ReadOnlySpan<RenderTreeFrame>)]
        );
    }

    public ArrayBuilderOfRenderTreeFrameWrapper(object wrapped)
        : base(wrapped)
    {
        Types.AssertIsInstanceOfType(WrappedType, wrapped);
    }

    public void Append(ReadOnlySpan<RenderTreeFrame> source)
    {
        var parameter = Expression.Parameter(typeof(ReadOnlySpan<RenderTreeFrame>), "source");
        var lambda = Expression.Lambda<Func<ReadOnlySpan<RenderTreeFrame>, int>>(
            Expression.Call(
                Expression.Constant(Inner),
                _Append,
                parameter
            ),
            parameter
        ).Compile();
        lambda(source);
    }
}
