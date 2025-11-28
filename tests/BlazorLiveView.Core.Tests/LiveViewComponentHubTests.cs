using BlazorLiveView.Core.Reflection;
using System.Reflection;

namespace BlazorLiveView.Core.Tests;

public class LiveViewComponentHubTests
{
    [Fact]
    public void LiveViewComponentHub_Methods_Match_ComponentHub()
    {
        var componentHubType = Types.ComponentHub;
        var liveViewComponentHubType = typeof(LiveViewComponentHub);

        var componentHubMethods = componentHubType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .OrderBy(m => m.Name)
            .ToArray();

        var liveViewComponentHubMethods = liveViewComponentHubType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .OrderBy(m => m.Name)
            .ToArray();

        Assert.Equal(componentHubMethods.Length, liveViewComponentHubMethods.Length);

        for (int i = 0; i < componentHubMethods.Length; i++)
        {
            var expected = componentHubMethods[i];
            var actual = liveViewComponentHubMethods[i];

            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.ReturnType, actual.ReturnType);

            var expectedParams = expected.GetParameters();
            var actualParams = actual.GetParameters();
            Assert.Equal(expectedParams.Length, actualParams.Length);

            for (int j = 0; j < expectedParams.Length; j++)
            {
                Assert.Equal(expectedParams[j].ParameterType, actualParams[j].ParameterType);
                Assert.Equal(expectedParams[j].Name, actualParams[j].Name);
            }
        }
    }
}
