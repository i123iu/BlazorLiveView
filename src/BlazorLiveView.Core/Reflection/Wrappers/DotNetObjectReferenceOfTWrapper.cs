using System.Reflection;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class DotNetObjectReferenceOfTWrapper(object inner)
    : WrapperBase(inner)
{
    private static readonly Type InnerType = Types.DotNetObjectReferenceOfT;
    private static readonly MethodInfo _Create;

    static DotNetObjectReferenceOfTWrapper()
    {
        _Create = Types.DotNetObjectReference
            .GetRequiredMethod("Create", isPublic: true, isStatic: true);
    }

    public static DotNetObjectReferenceOfTWrapper Create(object value)
    {
        var genericCreate = _Create.MakeGenericMethod(value.GetType());
        var inner = genericCreate.Invoke(null, new object[] { value })!;
        return new DotNetObjectReferenceOfTWrapper(inner);
    }

    public object Value
    {
        get
        {
            var propertyInfo = InnerType
                .MakeGenericType(Inner.GetType().GetGenericArguments())
                .GetRequiredProperty("Value", isPublic: true);
            return propertyInfo.GetValue(Inner)!;
        }
    }
}
