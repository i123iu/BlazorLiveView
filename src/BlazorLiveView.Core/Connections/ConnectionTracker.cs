
using BlazorLiveView.Core.Circuits;

namespace BlazorLiveView.Core.Connections;

// Connection ID = Circuit ID

internal class ConnectionTracker : IConnectionTracker
{
    public event IConnectionTracker.ConnectionOpenedHandler? ConnectionOpened;
    public event IConnectionTracker.ConnectionClosedHandler? ConnectionClosed;

    private readonly ICircuitTracker _circuitTracker;

    private readonly Lock _lock = new();
    private readonly Dictionary<string, IConnection> _connections = new();

    public ConnectionTracker(ICircuitTracker circuitTracker)
    {
        _circuitTracker = circuitTracker;
        _circuitTracker.CircuitOpenedEvent += OnCircuitOpened;
        _circuitTracker.CircuitClosedEvent += OnCircuitClosed;
        lock (_lock)
        {
            foreach (var circuit in _circuitTracker.ListCircuits().OfType<IUserCircuit>())
            {
                var connection = new Connection(circuit);
                _connections[circuit.Id] = connection;
                ConnectionOpened?.Invoke(connection);
            }
        }
    }

    public IConnection? GetConnection(string id)
    {
        lock (_lock)
        {
            _connections.TryGetValue(id, out var connection);
            return connection;
        }
    }

    public IEnumerable<IConnection> ListConnections()
    {
        lock (_lock)
        {
            return _connections.Values.ToArray();
        }
    }

    private void OnCircuitOpened(ICircuit circuit)
    {
        lock (_lock)
        {
            if (circuit is not IUserCircuit userCircuit)
                return;

            if (_connections.ContainsKey(userCircuit.Id))
            {
                throw new InvalidOperationException(
                    $"Connection with ID '{userCircuit.Id}' already exists."
                );
            }

            var connection = new Connection(userCircuit);
            _connections[userCircuit.Id] = connection;
            ConnectionOpened?.Invoke(connection);
        }
    }

    private void OnCircuitClosed(ICircuit circuit)
    {
        lock (_lock)
        {
            if (circuit is not IUserCircuit userCircuit)
                return;

            if (!_connections.Remove(userCircuit.Id, out var connection))
            {
                throw new InvalidOperationException(
                    $"Connection with ID '{userCircuit.Id}' does not exist."
                );
            }

            ConnectionClosed?.Invoke(connection);
        }
    }
}
