using BlazorLiveView.Core.Circuits;

namespace BlazorLiveView.Core.Connections;

// Connection ID = Circuit ID

internal class Connection : IConnection, IDisposable
{
    private readonly IUserCircuit _circuit;

    public event IConnection.UriChangedHandler? UriChanged;
    public event IConnection.ConnectionStatusChangedHandler? ConnectionStatusChanged;

    public Connection(IUserCircuit circuit)
    {
        _circuit = circuit;
        _circuit.UriChanged += OnCircuitUriChanged;
        _circuit.CircuitStatusChanged += OnCircuitStatusChanged;
    }

    public void Dispose()
    {
        _circuit.UriChanged -= OnCircuitUriChanged;
        _circuit.CircuitStatusChanged -= OnCircuitStatusChanged;
    }

    public string Id => _circuit.Id;
    public ConnectionStatus ConnectionStatus => _circuit.CircuitStatus switch
    {
        CircuitStatus.Open => ConnectionStatus.Connected,
        CircuitStatus.Up => ConnectionStatus.Connected,
        CircuitStatus.Down => ConnectionStatus.Reconnecting,
        CircuitStatus.Closed => ConnectionStatus.Disconnected,
        _ => throw new ArgumentOutOfRangeException()
    };
    public string Uri => _circuit.Uri;
    public DateTime ConnectedAt => _circuit.OpenedAt;

    private void OnCircuitUriChanged(IUserCircuit circuit)
    {
        if (!ReferenceEquals(circuit, _circuit)) throw new InvalidOperationException();
        UriChanged?.Invoke(this);
    }

    private void OnCircuitStatusChanged(ICircuit circuit)
    {
        if (!ReferenceEquals(circuit, _circuit)) throw new InvalidOperationException();
        ConnectionStatusChanged?.Invoke(this);
    }
}
