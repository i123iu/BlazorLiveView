using Microsoft.JSInterop;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class RemoteJSRuntimeWrapper : WrapperBase<JSRuntime>
{
    private static readonly Type InnerType = Types.RemoteJSRuntime;

    static RemoteJSRuntimeWrapper()
    { }

    public RemoteJSRuntimeWrapper(JSRuntime inner)
        : base(inner)
    {
        Types.AssertIsInstanceOfType(InnerType, inner);
    }
}