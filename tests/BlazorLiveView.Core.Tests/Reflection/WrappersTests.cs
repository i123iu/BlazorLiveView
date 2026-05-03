using BlazorLiveView.Core.Reflection;

namespace BlazorLiveView.Core.Tests.Reflection;

public class WrappersTests
{
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
        var wrapperTypes = typeof(WrapperBase).Assembly
            .GetTypes()
            .Where(t => t.IsClass &&
                !t.IsAbstract && typeof(WrapperBase).IsAssignableFrom(t) &&
                t != typeof(WrapperBase))
            .OrderBy(t => t.Name);

        Assert.NotEmpty(wrapperTypes);

        foreach (var wrapperType in wrapperTypes)
        {
            // Run class (static) constructor
            wrapperType.TypeInitializer!.Invoke(null, []);
        }
    }
}
