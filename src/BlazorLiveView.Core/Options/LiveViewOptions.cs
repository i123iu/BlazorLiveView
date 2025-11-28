namespace BlazorLiveView.Core.Options;

public sealed class LiveViewOptions
{
    public const string DEFAULT_LIVEVIEW_PATH_PREFIX = "/_liveview";
    public const string DEFAULT_MIRROR_PATH = "/mirror";
    public const string DEFAULT_HUB_PATH = "/hub";
    public const string DEFAULT_JS_PATH = "/blazor.web.js";

    private string _liveviewPathPrefix = DEFAULT_LIVEVIEW_PATH_PREFIX;
    private string _mirrorPath = DEFAULT_MIRROR_PATH;
    private string _hubPath = DEFAULT_HUB_PATH;
    private string _jsPath = DEFAULT_JS_PATH;

    public string LiveViewPathPrefix
    {
        get => _liveviewPathPrefix;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    $"{nameof(LiveViewPathPrefix)} cannot be null or whitespace.", nameof(value));
            }
            if (!value.StartsWith('/'))
            {
                throw new ArgumentException(
                    $"{nameof(LiveViewPathPrefix)} must start with a '/'.", nameof(value));
            }
            if (value.EndsWith('/'))
            {
                throw new ArgumentException(
                    $"{nameof(LiveViewPathPrefix)} cannot end with a '/'.", nameof(value));
            }

            _liveviewPathPrefix = value;
        }
    }

    /// <summary>
    /// URI path of the mirror endpoint.
    /// </summary>
    public string MirrorUri => _liveviewPathPrefix + _mirrorPath;

    /// <summary>
    /// URI path of the custom ComponentHub SignalR endpoint.
    /// </summary>
    public string HubUri => _liveviewPathPrefix + _hubPath;

    /// <summary>
    /// URI path of the JavaScript file for mirror circuits.
    /// </summary>
    public string JsUri => _liveviewPathPrefix + _jsPath;

    /// <summary>
    /// If set to true, a div element will be placed over the screen of all
    /// mirror circuits to disable interacting with the app. This is only a
    /// precaution, as interactions should not do anything and be ignored. 
    /// The user of the mirror circuit can still scroll normally. 
    /// </summary>
    public bool UseScreenOverlay { get; set; } = true;
}