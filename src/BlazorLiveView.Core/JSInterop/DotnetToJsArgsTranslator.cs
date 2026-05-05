using BlazorLiveView.Core.Reflection;
using BlazorLiveView.Core.Reflection.Wrappers;
using Microsoft.Extensions.Logging;

namespace BlazorLiveView.Core.JSInterop;

internal class DotnetToJsArgsTranslator(
    ILogger<DotnetToJsArgsTranslator> logger
) : IDotnetToJsArgsTranslator
{
    private readonly ILogger _logger = logger;

    public object?[] TranslateArgs(string identifier, object?[] args)
    {
        object?[] newArgs = new object?[args.Length];

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg == null) continue;

            if (Types.IDotNetObjectReference.IsInstanceOfType(arg))
            {
                if (arg.GetType().GetGenericTypeDefinition().IsAssignableTo(Types.DotNetObjectReferenceOfT))
                {
                    // An instance of DotNetObjectReference<T> contains a
                    // reference to the original JSRuntime object. So a new
                    // instance must be crated with the same value.
                    var wrapper = new DotNetObjectReferenceOfTWrapper(arg);
                    var refValue = wrapper.Value;
                    var newRef = DotNetObjectReferenceOfTWrapper.Create(refValue);
                    newArgs[i] = newRef.Inner;
                }
                else
                {
                    _logger.LogWarning(
                        "When calling {Identifier}: expected argument of type {ArgType} to be of type DotNetObjectReference<T> when implements interface IDotNetObjectReference",
                        identifier, arg.GetType().Name
                    );
                }
            }
            else
            {
                newArgs[i] = arg;
            }
        }

        return newArgs;
    }
}
