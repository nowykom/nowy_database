namespace Nowy.Database.Contract.Services;

public interface IMessageHubPeer
{
    string[] Names { get; }
    NowyMessageHubPeerType Type { get; }
}
