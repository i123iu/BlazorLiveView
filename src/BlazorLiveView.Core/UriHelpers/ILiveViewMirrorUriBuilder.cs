using System.Diagnostics.CodeAnalysis;

namespace BlazorLiveView.Core.UriHelpers;

public interface ILiveViewMirrorUriBuilder
{
    string GetPathAndQuery(MirrorUri mirrorUri);
    bool TryParse(Uri uri, [NotNullWhen(true)] out MirrorUri mirrorUri);
}
