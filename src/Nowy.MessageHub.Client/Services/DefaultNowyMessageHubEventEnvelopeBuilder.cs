using Nowy.Database.Contract.Models;

namespace Nowy.MessageHub.Client.Services;

internal class DefaultNowyMessageHubEventEnvelopeBuilder : INowyMessageHubEventEnvelopeBuilder
{
    private readonly List<string> _recipients = new();
    private readonly List<object> _values = new();

    public void AddRecipient(string name)
    {
        this._recipients.Add(name);
    }

    public void AddValue(object value)
    {
        this._values.Add(value);
    }

    public IReadOnlyList<string> Recipients => _recipients;

    public IReadOnlyList<object> Values => _values;
}
