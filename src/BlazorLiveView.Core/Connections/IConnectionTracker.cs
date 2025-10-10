namespace BlazorLiveView.Core.Connections;

public interface IConnectionTracker
{
    public delegate void ConnectionOpenedHandler(IConnection connection);
    public delegate void ConnectionClosedHandler(IConnection connection);

    event ConnectionOpenedHandler? ConnectionOpened;
    event ConnectionClosedHandler? ConnectionClosed;

    IConnection? GetConnection(string id);
    IEnumerable<IConnection> ListConnections();
}
