using BlazorLiveView.Core.Reflection;
using BlazorLiveView.Core.Reflection.Wrappers;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.SignalR;

namespace BlazorLiveView.Core;

internal class LiveViewComponentHub : Hub
{
    private readonly ComponentHubWrapper _inner;

    public LiveViewComponentHub(IServiceProvider services)
    {
        _inner = ComponentHubWrapper.Constructor(
            services.GetService(Types.IServerComponentDeserializer) ?? throw new NullReferenceException(),
            (services.GetService(typeof(IDataProtectionProvider)) as IDataProtectionProvider) ?? throw new NullReferenceException(),
            services.GetService(Types.ICircuitFactory) ?? throw new NullReferenceException(),
            services.GetService(Types.CircuitIdFactory) ?? throw new NullReferenceException(),
            services.GetService(Types.CircuitRegistry) ?? throw new NullReferenceException(),
            services.GetService(Types.ICircuitHandleRegistry) ?? throw new NullReferenceException(),
            services.GetService(Types.ILoggerOfComponentHub) ?? throw new NullReferenceException()
        );
    }

    public override Task OnDisconnectedAsync(Exception? exception)
        => _inner.OnDisconnectedAsync(exception);

    public ValueTask<string> StartCircuit(string baseUri, string uri, string serializedComponentRecords, string applicationState)
        => _inner.StartCircuit(baseUri, uri, serializedComponentRecords, applicationState);

    public Task UpdateRootComponents(string serializedComponentOperations, string applicationState)
        => _inner.UpdateRootComponents(serializedComponentOperations, applicationState);

    public ValueTask<bool> ConnectCircuit(string circuitIdSecret)
        => _inner.ConnectCircuit(circuitIdSecret);

    public ValueTask BeginInvokeDotNetFromJS(string callId, string assemblyName, string methodIdentifier, long dotNetObjectId, string argsJson)
        => _inner.BeginInvokeDotNetFromJS(callId, assemblyName, methodIdentifier, dotNetObjectId, argsJson);

    public ValueTask EndInvokeJSFromDotNet(long asyncHandle, bool succeeded, string arguments)
        => _inner.EndInvokeJSFromDotNet(asyncHandle, succeeded, arguments);

    public ValueTask ReceiveByteArray(int id, byte[] data)
        => _inner.ReceiveByteArray(id, data);

    public ValueTask<bool> ReceiveJSDataChunk(long streamId, long chunkId, byte[] chunk, string error)
        => _inner.ReceiveJSDataChunk(streamId, chunkId, chunk, error);

    public IAsyncEnumerable<ArraySegment<byte>> SendDotNetStreamToJS(long streamId)
        => _inner.SendDotNetStreamToJS(streamId);

    public ValueTask OnRenderCompleted(long renderId, string errorMessageOrNull)
        => _inner.OnRenderCompleted(renderId, errorMessageOrNull);

    public ValueTask OnLocationChanged(string uri, string? state, bool intercepted)
        => _inner.OnLocationChanged(uri, state, intercepted);

    public ValueTask OnLocationChanging(int callId, string uri, string? state, bool intercepted)
        => _inner.OnLocationChanging(callId, uri, state, intercepted);
}
