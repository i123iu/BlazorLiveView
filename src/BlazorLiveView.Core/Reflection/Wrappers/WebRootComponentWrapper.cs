using System.Reflection;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class WebRootComponentWrapper
    : WrapperBase
{
    private static readonly Type InnerType = Types.WebRootComponent;
    private static readonly FieldInfo _interactiveComponentId;

    static WebRootComponentWrapper()
    {
        _interactiveComponentId = InnerType.GetRequiredField("_interactiveComponentId", isPublic: false);
    }

    public WebRootComponentWrapper(object inner)
        : base(inner)
    {
        Types.AssertIsInstanceOfType(InnerType, inner);
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