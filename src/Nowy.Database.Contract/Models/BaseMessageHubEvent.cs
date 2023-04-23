using Nowy.Database.Contract.Services;

namespace Nowy.Database.Contract.Models;

public class BaseMessageHubEvent : IMessageHubEvent
{
    public IMessageHubPeer Sender { get; }
}
