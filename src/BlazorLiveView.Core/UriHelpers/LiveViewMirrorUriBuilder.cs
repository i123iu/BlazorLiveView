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
        sb.Append($"circuitId={Uri.EscapeDataString(mirrorUri.sourceCircuitId)}");
        return sb.ToString();
    }

    public bool TryParse(Uri uri, [NotNullWhen(true)] out MirrorUri mirrorUri)
    {
        var path = uri.AbsolutePath.AsSpan().TrimEnd('/');
        var expectedPath = _options.MirrorUri.AsSpan().TrimEnd('/');
        if (!path.Equals(expectedPath, StringComparison.OrdinalIgnoreCase))
        {
            mirrorUri = default;
            return false;
        }

        var query = HttpUtility.ParseQueryString(uri.Query);
        var circuitId = query.Get("circuitId");
        if (string.IsNullOrEmpty(circuitId))
        {
            mirrorUri = default;
            return false;
        }

        mirrorUri = new MirrorUri(circuitId);
        return true;
    }
}
