using System.Reflection;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class WebRootComponentWrapper
    : WrapperBase
{
    private static readonly Type WrappedType = Types.WebRootComponent;
    private static readonly FieldInfo _interactiveComponentId;

    static WebRootComponentWrapper()
    {
        _interactiveComponentId = WrappedType.GetRequiredField("_interactiveComponentId", isPublic: false);
    }

    public WebRootComponentWrapper(object wrapped)
        : base(wrapped)
    {
        Types.AssertIsInstanceOfType(WrappedType, wrapped);
    }

    public int InteractiveComponentId
    {
        get
        {
            object value = _interactiveComponentId.GetValue(Inner)
                ?? throw new Exception("_interactiveComponentId is null.");
            return (int)value;
        }
    }
}