using BlazorLiveView.Core.JSInterop;
using BlazorLiveView.Core.Reflection.Wrappers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;

namespace BlazorLiveView.Core.Tests.JSInterop;

public class DotnetToJsArgsTranslatorTests
{
    private const string identifier = "identifier";

    private static object?[] TranslateArgs(
        object?[] args
    )
    {
        DotnetToJsArgsTranslator translator = new(
            NullLogger<DotnetToJsArgsTranslator>.Instance
        );
        return translator.TranslateArgs(identifier, args);
    }

    [Fact]
    public void TranslateArgs_CommonTypes()
    {
        object obj = new();
        object?[] args = ["test", 123, true, obj];

        var translated = TranslateArgs(args);
        Assert.Collection(translated,
            arg => Assert.Equal("test", arg),
            arg => Assert.Equal(123, arg),
            arg => Assert.Equal(true, arg),
            arg => Assert.Same(obj, arg)
        );
    }

    [Fact]
    public void TranslateArgs_DotNetObjectReference()
    {
        var obj = new object();
        var reference = DotNetObjectReference.Create(obj);

        var translated = TranslateArgs([reference]);
        var cloned = Assert.Single(translated);
        Assert.NotNull(cloned);
        Assert.NotEqual(reference, cloned);
        DotNetObjectReferenceOfTWrapper wrapper = new(cloned);
        Assert.Equal(obj, wrapper.Value);
    }
}
