namespace BlazorLiveView.Core.Circuits;

/// <summary>
/// Connection to a client, either a <see cref="IUserCircuit"/> or a <see cref="IMirrorCircuit"/>.
/// </summary>
internal interface ICircuit
{
    public delegate void CircuitStatusChangedHandler(ICircuit circuit);

    event CircuitStatusChangedHandler? CircuitStatusChanged;

    string Id { get; }
    CircuitStatus CircuitStatus { get; }
    DateTime OpenedAt { get; }

    void SetUp();
    void SetDown();
    void SetClosed();
}
