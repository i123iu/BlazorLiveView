using BlazorLiveView.Core.Options;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Web;

namespace BlazorLiveView.Core.UriHelpers;

internal class LiveViewMirrorUriBuilder(
    IOptions<LiveViewOptions> options
) : ILiveViewMirrorUriBuilder
{
    private readonly LiveViewOptions _options = options.Value;

    public string GetPathAndQuery(MirrorUri mirrorUri)
    {
        var sb = new StringBuilder();
        sb.Append(_options.MirrorUri);
        sb.Append('?');
        sb.Append($"{nameof(MirrorUri.sourceCircuitId)}={Uri.EscapeDataString(mirrorUri.sourceCircuitId)}");
        if (mirrorUri.parentCircuitId is not null)
        {
            sb.Append('&');
            sb.Append($"{nameof(MirrorUri.parentCircuitId)}={Uri.EscapeDataString(mirrorUri.parentCircuitId)}");
        }
        if (mirrorUri.debugView)
        {
            sb.Append('&');
            sb.Append($"{nameof(MirrorUri.debugView)}=1");
        }
        return sb.ToString();
    }

    public bool TryParse(Uri uri, [NotNullWhen(true)] out MirrorUri mirrorUri)
    {
        var path = uri.AbsolutePath.AsSpan().TrimEnd('/');
        var expectedPath = _options.MirrorUri.AsSpan();
        if (!path.Equals(expectedPath, StringComparison.OrdinalIgnoreCase))
        {
            mirrorUri = default;
            return false;
        }

        var query = HttpUtility.ParseQueryString(uri.Query);

        var circuitId = query.Get(nameof(MirrorUri.sourceCircuitId));
        if (string.IsNullOrEmpty(circuitId))
        {
            mirrorUri = default;
            return false;
        }

        var parentCircuitId = query.Get(nameof(MirrorUri.parentCircuitId));
        if (string.IsNullOrEmpty(parentCircuitId))
        {
            parentCircuitId = null;
        }

        var debugViewStr = query.Get(nameof(MirrorUri.debugView));
        var debugView = false;
        if (!string.IsNullOrEmpty(debugViewStr) && debugViewStr.Equals("1"))
        {
            debugView = true;
        }

        mirrorUri = new MirrorUri(
            circuitId, 
            parentCircuitId, 
            debugView
        );
        return true;
    }
}
