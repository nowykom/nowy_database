namespace Nowy.Database.Contract.Models;

public interface IUniqueModel : IBaseModel
{
    string GetKey();
}
