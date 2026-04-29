using Microsoft.JSInterop;
using System.Reflection;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class RemoteJSRuntimeWrapper : WrapperBase<JSRuntime>
{
    private static readonly Type InnerType = Types.RemoteJSRuntime;
    private static readonly PropertyInfo _IsInitialized;

    static RemoteJSRuntimeWrapper()
    {
        _IsInitialized = InnerType.GetRequiredProperty(
            "IsInitialized", isPublic: true
        );
    }

    public RemoteJSRuntimeWrapper(JSRuntime inner)
        : base(inner)
    {
        Types.AssertIsInstanceOfType(InnerType, inner);
    }

    public bool IsInitialized => (bool)_IsInitialized.GetValue(Inner)!;
}
