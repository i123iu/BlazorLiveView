namespace BlazorLiveView.Core.Connections;

public interface IConnection
{
    public delegate void UriChangedHandler(IConnection connection);
    public delegate void ConnectionStatusChangedHandler(IConnection connection);

    event UriChangedHandler? UriChanged;
    event ConnectionStatusChangedHandler? ConnectionStatusChanged;

    string Id { get; }
    ConnectionStatus ConnectionStatus { get; }
    DateTime ConnectedAt { get; }
    string Uri { get; }
}
