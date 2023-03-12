using Nowy.Database.Contract.Models;

namespace Nowy.Database.Client.Services;

public interface INowyCollection<TModel> where TModel : class, IBaseModel
{
    Task<IReadOnlyList<TModel>> GetAllAsync();
    Task<TModel?> GetByIdAsync(string uuid);
    Task<TModel> InsertAsync(string uuid, TModel model);
    Task<TModel> UpdateAsync(string uuid, TModel model);
    Task DeleteAsync(string uuid);
}

public sealed class NowyDatabaseException : Exception
{
}
