namespace Nowy.Database.Contract.Services;

public interface INowyMessageHubReceiverPayload
{
    public TValue? GetValue<TValue>() where TValue : class;
}
