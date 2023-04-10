using Microsoft.Extensions.DependencyInjection;
using Nowy.Database.Common.Models;
using Nowy.Database.Contract.Models;
using Nowy.Database.Contract.Services;

namespace Nowy.Database.Client.Tests.Tests;

public class UnitTest1
{
    private ServiceProvider _sp;

    public UnitTest1()
    {
        ServiceCollection services = new();

        services.AddHttpClient();

        string endpoint = "https://main.database.nowykom.de";
        services.AddNowyDatabaseClient(endpoint: endpoint);

        this._sp = services.BuildServiceProvider();
    }

    [Fact]
    public async Task TestInsert()
    {
        INowyDatabase database = this._sp.GetRequiredService<INowyDatabase>();

        INowyCollection<TestModel> collection = database.GetCollection<TestModel>("unit_tests");

        TestModel a;

        a = await collection.InsertAsync("123", new TestModel() { Fuck = "A" });
        Assert.Equal("A", a.Fuck);

        a = await collection.InsertAsync("123", new TestModel() { Fuck = "B" });
        Assert.Equal("B", a.Fuck);
    }

    [Fact]
    public async Task TestUpdate()
    {
        INowyDatabase database = this._sp.GetRequiredService<INowyDatabase>();

        INowyCollection<TestModel> collection = database.GetCollection<TestModel>("unit_tests");

        TestModel a;

        a = await collection.UpdateAsync("123", new TestModel() { Fuck = "C" });
        Assert.Equal("C", a.Fuck);

        a = await collection.UpdateAsync("123", new TestModel() { Fuck = "D" });
        Assert.Equal("D", a.Fuck);
    }

    [Fact]
    public async Task TestDelete()
    {
        INowyDatabase database = this._sp.GetRequiredService<INowyDatabase>();

        INowyCollection<TestModel> collection = database.GetCollection<TestModel>("unit_tests");

        TestModel? a;

        await collection.DeleteAsync("123");

        a = await collection.GetByIdAsync("123");
        Assert.Null(a);

        a = await collection.InsertAsync("123", new TestModel() { Fuck = "XYZ" });
        Assert.Equal("XYZ", a.Fuck);

        a = await collection.GetByIdAsync("123");
        Assert.NotNull(a);
        Assert.Equal("XYZ", a.Fuck);

        a = await collection.UpdateAsync("123", new TestModel() { Fuck = "C" });
        Assert.Equal("C", a.Fuck);
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
