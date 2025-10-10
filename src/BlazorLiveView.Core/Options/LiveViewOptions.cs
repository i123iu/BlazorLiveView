namespace BlazorLiveView.Core.Options;

public sealed class LiveViewOptions
{
    public const string DEFAULT_MIRROR_URI = "/_mirror";

    /// <summary>
    /// The URI path where users (admins) using <c>BlazorLiveView</c> can access the live view. 
    /// </summary>
    public string MirrorUri { get; set; } = DEFAULT_MIRROR_URI;
}