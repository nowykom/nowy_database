namespace Nowy.Database.Contract.Models;

public interface IDeletableModel
{
    bool is_deleted { get; set; }
}
