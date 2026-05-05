namespace BlazorLiveView.Core.JSInterop;

internal interface IDotnetToJsArgsTranslator
{
    object?[] TranslateArgs(string identifier, object?[] args);
}
