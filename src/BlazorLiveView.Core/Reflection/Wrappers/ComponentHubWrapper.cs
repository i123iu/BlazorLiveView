using Microsoft.AspNetCore.DataProtection;
using System.Reflection;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class ComponentHubWrapper
    : WrapperBase
{
    private static readonly Type InnerType = Types.ComponentHub;
    private static readonly ConstructorInfo _Constructor;
    private static readonly MethodInfo _OnDisconnectedAsync;
    private static readonly MethodInfo _StartCircuit;
    private static readonly MethodInfo _UpdateRootComponents;
    private static readonly MethodInfo _ConnectCircuit;
    private static readonly MethodInfo _BeginInvokeDotNetFromJS;
    private static readonly MethodInfo _EndInvokeJSFromDotNet;
    private static readonly MethodInfo _ReceiveByteArray;
    private static readonly MethodInfo _ReceiveJSDataChunk;
    private static readonly MethodInfo _SendDotNetStreamToJS;
    private static readonly MethodInfo _OnRenderCompleted;
    private static readonly MethodInfo _OnLocationChanged;
    private static readonly MethodInfo _OnLocationChanging;
    private static readonly MethodInfo _GetActiveCircuitAsync;

    static ComponentHubWrapper()
    {
        _Constructor = InnerType.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Single();
        _OnDisconnectedAsync = InnerType.GetRequiredMethod("OnDisconnectedAsync", isPublic: true);
        _StartCircuit = InnerType.GetRequiredMethod("StartCircuit", isPublic: true);
        _UpdateRootComponents = InnerType.GetRequiredMethod("UpdateRootComponents", isPublic: true);
        _ConnectCircuit = InnerType.GetRequiredMethod("ConnectCircuit", isPublic: true);
        _BeginInvokeDotNetFromJS = InnerType.GetRequiredMethod("BeginInvokeDotNetFromJS", isPublic: true);
        _EndInvokeJSFromDotNet = InnerType.GetRequiredMethod("EndInvokeJSFromDotNet", isPublic: true);
        _ReceiveByteArray = InnerType.GetRequiredMethod("ReceiveByteArray", isPublic: true);
        _ReceiveJSDataChunk = InnerType.GetRequiredMethod("ReceiveJSDataChunk", isPublic: true);
        _SendDotNetStreamToJS = InnerType.GetRequiredMethod("SendDotNetStreamToJS", isPublic: true);
        _OnRenderCompleted = InnerType.GetRequiredMethod("OnRenderCompleted", isPublic: true);
        _OnLocationChanged = InnerType.GetRequiredMethod("OnLocationChanged", isPublic: true);
        _OnLocationChanging = InnerType.GetRequiredMethod("OnLocationChanging", isPublic: true);
        _GetActiveCircuitAsync = InnerType.GetRequiredMethod("GetActiveCircuitAsync", isPublic: false);
    }

    public ComponentHubWrapper(object inner)
        : base(inner)
    {
        Types.AssertIsInstanceOfType(InnerType, inner);
    }

    public static ComponentHubWrapper Constructor(
        object serializer,
        IDataProtectionProvider dataProtectionProvider,
        object circuitFactory,
        object circuitIdFactory,
        object circuitRegistry,
        object circuitHandleRegistry,
        object logger
    )
    {
        Types.AssertIsInstanceOfType(Types.IServerComponentDeserializer, serializer);
        Types.AssertIsInstanceOfType(Types.ICircuitFactory, circuitFactory);
        Types.AssertIsInstanceOfType(Types.CircuitIdFactory, circuitIdFactory);
        Types.AssertIsInstanceOfType(Types.CircuitRegistry, circuitRegistry);
        Types.AssertIsInstanceOfType(Types.ICircuitHandleRegistry, circuitHandleRegistry);
        Types.AssertIsInstanceOfType(Types.ILoggerOfComponentHub, logger);

        return new ComponentHubWrapper(
            _Constructor.Invoke([
                serializer,
                dataProtectionProvider,
                circuitFactory,
                circuitIdFactory,
                circuitRegistry,
                circuitHandleRegistry,
                logger
            ])
        );
    }

    public Task OnDisconnectedAsync(Exception? exception)
        => (Task)_OnDisconnectedAsync.Invoke(Inner, [exception])!;

    public ValueTask<string> StartCircuit(string baseUri, string uri, string serializedComponentRecords, string applicationState)
        => (ValueTask<string>)_StartCircuit.Invoke(Inner, [baseUri, uri, serializedComponentRecords, applicationState])!;

    public Task UpdateRootComponents(string serializedComponentOperations, string applicationState)
        => (Task)_UpdateRootComponents.Invoke(Inner, [serializedComponentOperations, applicationState])!;

    public ValueTask<bool> ConnectCircuit(string circuitIdSecret)
        => (ValueTask<bool>)_ConnectCircuit.Invoke(Inner, [circuitIdSecret])!;

    public ValueTask BeginInvokeDotNetFromJS(string callId, string assemblyName, string methodIdentifier, long dotNetObjectId, string argsJson)
        => (ValueTask)_BeginInvokeDotNetFromJS.Invoke(Inner, [callId, assemblyName, methodIdentifier, dotNetObjectId, argsJson])!;

    public ValueTask EndInvokeJSFromDotNet(long asyncHandle, bool succeeded, string arguments)
        => (ValueTask)_EndInvokeJSFromDotNet.Invoke(Inner, [asyncHandle, succeeded, arguments])!;

    public ValueTask ReceiveByteArray(int id, byte[] data)
        => (ValueTask)_ReceiveByteArray.Invoke(Inner, [id, data])!;

    public ValueTask<bool> ReceiveJSDataChunk(long streamId, long chunkId, byte[] chunk, string error)
        => (ValueTask<bool>)_ReceiveJSDataChunk.Invoke(Inner, [streamId, chunkId, chunk, error])!;

    public IAsyncEnumerable<ArraySegment<byte>> SendDotNetStreamToJS(long streamId)
        => (IAsyncEnumerable<ArraySegment<byte>>)_SendDotNetStreamToJS.Invoke(Inner, [streamId])!;

    public ValueTask OnRenderCompleted(long renderId, string errorMessageOrNull)
        => (ValueTask)_OnRenderCompleted.Invoke(Inner, [renderId, errorMessageOrNull])!;

    public ValueTask OnLocationChanged(string uri, string? state, bool intercepted)
        => (ValueTask)_OnLocationChanged.Invoke(Inner, [uri, state, intercepted])!;

    public ValueTask OnLocationChanging(int callId, string uri, string? state, bool intercepted)
        => (ValueTask)_OnLocationChanging.Invoke(Inner, [callId, uri, state, intercepted])!;

    public ValueTask<object?> GetActiveCircuitAsync()
        => (ValueTask<object?>)_GetActiveCircuitAsync.Invoke(Inner, null)!;
}
