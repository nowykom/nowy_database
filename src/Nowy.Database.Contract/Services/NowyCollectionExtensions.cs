using Microsoft.Extensions.Logging;
using Nowy.Database.Contract.Models;

namespace Nowy.Database.Contract.Services;

public delegate void EnsureModelsExistUpdateFunction<in TModel>(TModel model_original, TModel model_input) where TModel : class, IBaseModel, IUniqueModel;

public static class NowyCollectionExtensions
{
    public static Task<IReadOnlyList<TModel>> GetByFilterAsync<TModel>(this INowyCollection<TModel> that, ModelFilterBuilder filter_builder) where TModel : class, IBaseModel
    {
        return that.GetByFilterAsync(filter: filter_builder.Build());
    }

    public static async Task<TModel?> GetByName<TModel>(this INowyCollection<TModel> that, string value) where TModel : class, IBaseModel
    {
        ModelFilterBuilder filter_builder = ModelFilterBuilder.Equals("name", value);
        return ( await that.GetByFilterAsync(filter: filter_builder.Build()) ).FirstOrDefault();
    }

    public static async Task<TModel?> GetByIds<TModel>(this INowyCollection<TModel> that, IEnumerable<string> values) where TModel : class, IBaseModel
    {
        ModelFilterBuilder filter_builder = ModelFilterBuilder.In(nameof(IBaseModel.ids), values);
        return ( await that.GetByFilterAsync(filter: filter_builder.Build()) ).FirstOrDefault();
    }

    public static async Task EnsureModelsExist<TModel>(
        this INowyCollection<TModel> collection,
        ILogger? logger,
        IReadOnlyList<TModel> items_previous,
        IReadOnlyList<TModel> items_input,
        bool soft_delete = true,
        EnsureModelsExistUpdateFunction<TModel>? update_func = null
    ) where TModel : class, IBaseModel, IUniqueModel
    {
        string model_name = typeof(TModel).Name;

        if (items_previous.Any(o => o.is_deleted))
        {
            items_previous = items_previous.Where(o => o.is_deleted == false).ToList();
        }

        if (items_input.Any(o => o.is_deleted))
        {
            items_input = items_input.Where(o => o.is_deleted == false).ToList();
        }

        Dictionary<string, TModel> items_input_by_key = items_input.ToDictionary(o => o.GetKey(), o => o);
        Dictionary<string, TModel> items_previous_by_key = items_previous.ToDictionary(o => o.GetKey(), o => o);

        logger?.LogInformation("Ensure {model_name}s exist: input = {count_items_input}", model_name, items_input.Count);
        logger?.LogInformation("Ensure {model_name}s exist: previous = {count_items_previous}", model_name, items_previous.Count);

        Dictionary<string, TModel> items_to_add = items_input_by_key
            .Where(kvp => !items_previous_by_key.ContainsKey(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        Dictionary<string, (TModel item_original, TModel item_input)> items_to_update = items_input_by_key
            .Select(kvp => ( Key: kvp.Key, ValueOriginal: items_previous_by_key!.Get(kvp.Key, null), ValueInput: kvp.Value ))
            .Where(o => o.ValueOriginal is { })
            .ToDictionary(o => o.Key, o => ( o.ValueOriginal!, o.ValueInput ));

        Dictionary<string, TModel> items_to_remove = items_previous_by_key
            .Where(kvp => !items_input_by_key.ContainsKey(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        logger?.LogInformation("Ensure {model_name}s exist: count items to add:    {count_items_to_add}", model_name, items_to_add.Count);
        logger?.LogInformation("Ensure {model_name}s exist: count items to update: {count_items_to_update}", model_name, items_to_update.Count);
        logger?.LogInformation("Ensure {model_name}s exist: count items to remove: {count_items_to_remove}", model_name, items_to_remove.Count);

        foreach (TModel item in items_to_add.Values)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            item.is_deleted = false;

            logger?.LogInformation("Ensure {model_name}s exist: add item: {@item}", model_name, item);

            await collection.UpsertAsync(item.id, item);
        }

        foreach (( TModel item_original, TModel item_input ) in items_to_update.Values)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global

            if (update_func is { })
            {
                item_original.is_deleted = false;
                item_input.is_deleted = false;

                logger?.LogInformation(
                    "Ensure {model_name}s exist: update item (copy some properties): {@item_original} => {@item_input}",
                    model_name,
                    item_original,
                    item_input
                );

                update_func(item_original, item_input);
                await collection.UpsertAsync(item_original.id, item_original);
            }
            else
            {
                item_input.is_deleted = false;

                logger?.LogInformation(
                    "Ensure {model_name}s exist: replace item with input: {@item_original} => {@item_input}",
                    model_name,
                    item_original,
                    item_input
                );

                await collection.UpsertAsync(item_input.id, item_input);
            }
        }

        foreach (TModel item in items_to_remove.Values)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (soft_delete)
            {
                logger?.LogInformation("Ensure {model_name}s exist: remove item: {@item}", model_name, item);

                item.is_deleted = true;
                await collection.UpsertAsync(item.id, item);
            }
            else
            {
                logger?.LogInformation("Ensure {model_name}s exist: remove item: {@item}", model_name, item);

                await collection.DeleteAsync(item.id);
            }
        }
    }
}