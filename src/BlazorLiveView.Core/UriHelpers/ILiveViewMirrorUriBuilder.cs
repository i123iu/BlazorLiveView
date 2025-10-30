using System.Diagnostics.CodeAnalysis;

namespace BlazorLiveView.Core.UriHelpers;

/// <summary>
/// Defines helper methods for building and parsing URIs for the Mirror Endpoint. 
/// See <see cref="MirrorUri"/>. 
/// </summary>
public interface ILiveViewMirrorUriBuilder
{
    string GetPathAndQuery(MirrorUri mirrorUri);
    bool TryParse(Uri uri, [NotNullWhen(true)] out MirrorUri mirrorUri);
}
