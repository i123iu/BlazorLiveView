namespace BlazorLiveView.Core.Options;

public sealed class LiveViewOptions
{
    public const string DEFAULT_MIRROR_URI = "/_mirror";

    private string _mirrorUri = DEFAULT_MIRROR_URI;

    /// <summary>
    /// The URI path where users (admins) using <c>BlazorLiveView</c> can access the live view. 
    /// </summary>
    public string MirrorUri
    {
        get => _mirrorUri;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("MirrorUri cannot be null or whitespace.", nameof(value));
            }
            if (!value.StartsWith('/'))
            {
                throw new ArgumentException("MirrorUri must start with a '/'.", nameof(value));
            }
            if (value.EndsWith('/'))
            {
                throw new ArgumentException("MirrorUri cannot end with a '/'.", nameof(value));
            }
            _mirrorUri = value;
        }
    }

    public string HubMirrorUri => MirrorUri + "/blazor.web.js";
    public string JsMirrorUri => MirrorUri + "/blazor.liveview.js";
}