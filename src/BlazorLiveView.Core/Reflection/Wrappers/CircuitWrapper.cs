using Microsoft.AspNetCore.Components.Server.Circuits;
using System.Reflection;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class CircuitWrapper(Circuit circuit)
    : WrapperBase<Circuit>(circuit)
{
    private static readonly Type InnerType = Types.Circuit;
    private static readonly FieldInfo _circuitHost;

    static CircuitWrapper()
    {
        _circuitHost = InnerType.GetRequiredField(
            "_circuitHost", isPublic: false
        );
    }

    public CircuitHostWrapper CircuitHost
        => new(_circuitHost.GetValue(Inner)!);
}
