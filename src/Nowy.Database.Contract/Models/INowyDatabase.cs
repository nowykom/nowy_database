namespace Nowy.Database.Contract.Models;

public interface INowyDatabase
{
    INowyCollection<TModel> GetCollection<TModel>(string database_name) where TModel : class, IBaseModel;
}