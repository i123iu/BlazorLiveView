namespace BlazorLiveView.Core.RenderTree;


[Serializable]
public class RenderTreeTranslationException : Exception
{
    public RenderTreeTranslationException() { }
    public RenderTreeTranslationException(string message) : base(message) { }
    public RenderTreeTranslationException(string message, Exception inner) : base(message, inner) { }
}
