using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nowy.Database.Common;
using Nowy.Database.Common.Models;
using Nowy.Database.Contract.Models;
using Nowy.Database.Contract.Services;
using Serilog;
using Xunit.Abstractions;

namespace Nowy.Database.Client.Tests.Tests;

public class UnitTest2
{
    private readonly ServiceProvider _sp;

    public UnitTest2(ITestOutputHelper output)
    {
        Log.Logger = new LoggerConfiguration()
            // add the xunit test output sink to the serilog logger
            // https://github.com/trbenning/serilog-sinks-xunit#serilog-sinks-xunit
            .WriteTo.TestOutput(output)
            .CreateLogger();

        ServiceCollection services = new();

        services.AddHttpClient();

        services.AddNowyMessageHubClient(config =>
        {
            config.AddEndpoint("https://main.messagehub.schulz.dev");
            config.AddReceiver<Receiver1>(sp => new Receiver1());
        });

        _sp = services.BuildServiceProvider();
    }

    private Receiver1 _receiver1 => _sp.GetRequiredService<Receiver1>();

    public class Receiver1 : INowyMessageHubReceiver
    {
        private static readonly string[] EventNames = new[] { "test1", "test3", };

        internal readonly List<(string event_name, INowyMessageHubReceiverPayload payload)> ReceivedBuffer = new();

        IEnumerable<string> INowyMessageHubReceiver.GetEventNamePrefixes() => EventNames;

        Task INowyMessageHubReceiver.ReceiveMessageAsync(string event_name, INowyMessageHubReceiverPayload payload)
        {
            ReceivedBuffer.Add(( event_name, payload ));
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task TestSend()
    {
        IHostedService[] hosted_services = _sp.GetRequiredService<IEnumerable<IHostedService>>().ToArray();
        Assert.NotEmpty(hosted_services);
        foreach (IHostedService hosted_service in hosted_services)
        {
            using CancellationTokenSource cts = new();
            await hosted_service.StartAsync(cts.Token);
        }

        try
        {
            INowyMessageHub hub = this._sp.GetRequiredService<INowyMessageHub>();

            await hub.WaitUntilConnectedAsync("test1", TimeSpan.FromMilliseconds(5000));

            await hub.BroadcastMessageAsync("abc", new List<int> { 1, 2, 3, });

            Assert.Empty(_receiver1.ReceivedBuffer);

            await hub.BroadcastMessageAsync("test1", new List<int> { 1, 2, 3, });

            for (int i = 0; i < 50; i++)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                if (_receiver1.ReceivedBuffer.Count != 0)
                    break;
            }

            Assert.NotEmpty(_receiver1.ReceivedBuffer);
        }
        finally
        {
            foreach (IHostedService hosted_service in hosted_services)
            {
                using CancellationTokenSource cts = new();
                await hosted_service.StopAsync(cts.Token);
            }
        }
    }

    public sealed class TestModel : BaseModel
    {
        public string? FuckTest { get; set; }
        public string? Fuck { get; set; }
        public string? Test1 { get; set; }
        public string? Test2 { get; set; }
        public string? Test3 { get; set; }
    }
}
