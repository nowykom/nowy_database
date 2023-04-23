using Nowy.Database.Contract.Models;

namespace Nowy.Database.Contract.Services;

public interface INowyCollection<TModel> where TModel : class, IBaseModel
{
    string DatabaseName { get; }
    string EntityName { get; }

    Task<IReadOnlyList<TModel>> GetAllAsync();
    Task<TModel?> GetByIdAsync(string id);
    Task<TModel> InsertAsync(string id, TModel model);
    Task<TModel> UpdateAsync(string id, TModel model);
    Task DeleteAsync(string id);
    INowyCollectionEventSubscription<TModel> Subscribe();
}

public sealed class NowyDatabaseException : Exception
{
}
