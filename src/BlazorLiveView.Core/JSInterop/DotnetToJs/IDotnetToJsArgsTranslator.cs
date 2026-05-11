namespace BlazorLiveView.Core.JSInterop.DotnetToJs;

internal interface IDotnetToJsArgsTranslator
{
    object?[] TranslateArgs(string identifier, object?[] args);
}
