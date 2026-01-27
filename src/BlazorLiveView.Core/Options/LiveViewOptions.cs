using System.Security.Claims;

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

    /// <summary>
    /// If set to true, a div element will be placed over the screen of all
    /// mirror circuits to disable interacting with the app. This is only a
    /// precaution, as interactions should not do anything and be ignored. 
    /// The user of the mirror circuit can still scroll normally. 
    /// </summary>
    public bool UseScreenOverlay { get; set; } = true;

    /// <summary>
    /// Function to extract a unique user identifier (<c>UserSelector</c>) from
    /// a <see cref="ClaimsPrincipal"/>. This user selector can be an email or
    /// some unique login name. By default, it extracts the first email claim
    /// (<see cref="ClaimTypes.Email"/>). Return null for users that are not
    /// logged in.
    /// </summary>
    public Func<ClaimsPrincipal, string?> UserSelectorExtractor { get; set; } =
        principal => principal.FindFirstValue(ClaimTypes.Email);

    /// <summary>
    /// How should user selectors be compared with each other. Default is
    /// <see cref="StringComparison.OrdinalIgnoreCase"/>.
    /// </summary>
    public StringComparison UserSelectorStringComparison { get; set; } =
        StringComparison.OrdinalIgnoreCase;

    public bool ShowDebugOptions { get; set; } = false;
}