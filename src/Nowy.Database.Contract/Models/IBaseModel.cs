namespace Nowy.Database.Contract.Models;

public interface IBaseModel
{
    string id { get; set; }
    IReadOnlyList<string> ids { get; set; }
}
