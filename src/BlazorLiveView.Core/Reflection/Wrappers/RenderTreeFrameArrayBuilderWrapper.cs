namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class RenderTreeFrameArrayBuilderWrapper
    : ArrayBuilderOfRenderTreeFrameWrapper
{
    private static readonly Type InnerType = Types.RenderTreeFrameArrayBuilder;

    public RenderTreeFrameArrayBuilderWrapper(object inner)
        : base(inner)
    {
        Types.AssertIsInstanceOfType(InnerType, inner);
    }
}
