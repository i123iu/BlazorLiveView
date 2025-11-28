using BlazorLiveView.Core.Reflection;

namespace BlazorLiveView.Core.Tests.Reflection;

public class TypesTests
{
    [Fact]
    public void TypesNotNull()
    {
        Assert.NotNull(Types.Circuit);
        Assert.NotNull(Types.CircuitHost);
        Assert.NotNull(Types.CircuitFactory);
        Assert.NotNull(Types.RemoteRenderer);
        Assert.NotNull(Types.RemoteNavigationManager);
        Assert.NotNull(Types.WebRootComponentManager);
        Assert.NotNull(Types.WebRootComponent);

        Assert.NotNull(Types.ValueTaskOfCircuitHost);
        Assert.NotNull(Types.TaskOfCircuitHost);
        Assert.NotNull(Types.FuncOfTaskOfCircuitHostAndCircuitHost);

        Assert.NotNull(Types.Renderer);
        Assert.NotNull(Types.WebRenderer);
        Assert.NotNull(Types.ArrayBuilderOfRenderTreeFrame);
        Assert.NotNull(Types.RenderTreeFrameArrayBuilder);

        Assert.NotNull(Types.ComponentState);
        Assert.NotNull(Types.RenderTreeBuilder);

        Assert.NotNull(Types.CircuitOptions);
        Assert.NotNull(Types.ComponentHub);
    }
}
