using BlazorLiveView.Core.Circuits;
using BlazorLiveView.Core.Circuits.Services;
using BlazorLiveView.Core.Options;
using BlazorLiveView.Core.UriHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Forwarder;

namespace BlazorLiveView.Core;

internal static class MirrorEndpoint
{
    internal static async Task HandleAsync(
        HttpContext context,
        [FromQuery(Name = nameof(MirrorUri.sourceCircuitId))] string circuitId,
        ICircuitTracker circuitTracker,
        IOptions<LiveViewOptions> liveViewOptions,
        IHttpForwarder httpForwarder
    )
    {
        var circuit = circuitTracker.GetCircuit(circuitId);
        
        if (circuit is null)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync(
                $"Circuit with ID '{circuitId}' not found. "
            );
            return;
        }

        if (circuit is not IUserCircuit userCircuit)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync(
                $"Cannot mirror a mirror circuit. "
            );
            return;
        }

        // The mirror endpoint should return the same page the mirrored user
        // is viewing including any authentication state.

        var httpClient = new HttpMessageInvoker(
            liveViewOptions.Value.MirrorEndpointForwardSocketsHttpHandler
        );

        var targetUri = userCircuit.Uri;
        var transformer = new MirrorRequestTransformer(targetUri);
        var requestOptions = new ForwarderRequestConfig
        {
            ActivityTimeout = TimeSpan.FromSeconds(100)
        };

        await httpForwarder.SendAsync(
            context, targetUri, httpClient, 
            requestOptions, transformer
        );
    }

    private sealed class MirrorRequestTransformer(
        string targetUri
    ) : HttpTransformer
    {
        public override ValueTask TransformRequestAsync(
            HttpContext httpContext,
            HttpRequestMessage proxyRequest,
            string destinationPrefix,
            CancellationToken cancellationToken
        )
        {
            // Ignore the incoming mirror endpoint URI
            proxyRequest.RequestUri = new Uri(targetUri);
            return base.TransformRequestAsync(
                httpContext, proxyRequest,
                destinationPrefix, cancellationToken
            );
        }
    }
}
