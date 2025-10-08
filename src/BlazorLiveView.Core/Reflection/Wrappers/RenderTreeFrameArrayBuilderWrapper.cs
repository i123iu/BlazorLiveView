namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class RenderTreeFrameArrayBuilderWrapper
    : ArrayBuilderOfRenderTreeFrameWrapper
{
    private static readonly Type WrappedType = Types.RenderTreeFrameArrayBuilder;

    static RenderTreeFrameArrayBuilderWrapper()
    {

    }

    public RenderTreeFrameArrayBuilderWrapper(object wrapped)
        : base(wrapped)
    {
        Types.AssertIsInstanceOfType(WrappedType, wrapped);
    }
}
