using Microsoft.AspNetCore.Components.RenderTree;
using System.Linq.Expressions;
using System.Reflection;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class ArrayBuilderOfRenderTreeFrameWrapper
    : WrapperBase
{
    private static readonly Type InnerType = Types.RenderTreeFrameArrayBuilder.BaseType!;
    private static readonly MethodInfo _Append;

    static ArrayBuilderOfRenderTreeFrameWrapper()
    {
        _Append = InnerType.GetRequiredMethod(
            "Append",
            isPublic: false,
            parameters: [typeof(ReadOnlySpan<RenderTreeFrame>)]
        );
    }

    public ArrayBuilderOfRenderTreeFrameWrapper(object inner)
        : base(inner)
    {
        Types.AssertIsInstanceOfType(InnerType, inner);
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
