using BlazorLiveView.Core.Reflection;
using System.Reflection;

namespace BlazorLiveView.Core.Tests.Reflection;

public class TypesTests
{
    [Fact]
    public void TypesNotNull()
    {
        var typesFields = typeof(Types)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(Type));

        Assert.NotEmpty(typesFields);

        foreach (var field in typesFields)
        {
            var value = field.GetValue(null);
            Assert.NotNull(value);
        }
    }
}
