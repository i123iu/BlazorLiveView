using BlazorLiveView.Core.Reflection;
using BlazorLiveView.Core.Reflection.Wrappers;

namespace BlazorLiveView.Core.Tests.Reflection;

public class WrappersTests
{
    private static void RunClassConstructor<T>()
        where T : WrapperBase
    {
        typeof(T).TypeInitializer!.Invoke(null, []);
    }

    /// <summary>
    /// FieldInfo, PropertyInfo and MethodInfo instances are create in static constructors of wrapper classes.
    /// If any of them is not found, these methods will throw an exception:
    /// <list type="bullet">
    ///     <item><see cref="Types.GetRequiredField"/></item>
    ///     <item><see cref="Types.GetRequiredMethod"/></item>
    ///     <item><see cref="Types.GetRequiredProperty"/></item>
    /// </list>
    /// </summary>
    [Fact]
    public void TestWrapperClassConstructors()
    {
        RunClassConstructor<ArrayBuilderOfRenderTreeFrameWrapper>();
        RunClassConstructor<CircuitHostWrapper>();
        RunClassConstructor<CircuitWrapper>();
        RunClassConstructor<ComponentStateWrapper>();
        RunClassConstructor<RemoteNavigationManagerWrapper>();
        RunClassConstructor<RemoteRendererWrapper>();
        RunClassConstructor<RendererWrapper>();
        RunClassConstructor<RenderTreeBuilderWrapper>();
        RunClassConstructor<RenderTreeFrameArrayBuilderWrapper>();
        RunClassConstructor<TaskOfCircuitHostWrapper>();
        RunClassConstructor<ValueTaskOfCircuitHostWrapper>();
        RunClassConstructor<WebRootComponentManagerWrapper>();
        RunClassConstructor<WebRootComponentWrapper>();
    }
}
