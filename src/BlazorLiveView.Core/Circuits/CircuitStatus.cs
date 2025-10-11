namespace BlazorLiveView.Core.Circuits;

public enum CircuitStatus
{
    /// <summary>
    /// When the SignalR connection has be established.
    /// </summary>
    Open,

    /// <summary>
    /// When the components have been initialized.
    /// </summary>
    Up,

    /// <summary>
    /// When the connection has been lost (can still be reconnected).
    /// </summary>
    Down,

    /// <summary>
    /// When the connection has been unrecoverably closed.
    /// </summary>
    Closed
}
