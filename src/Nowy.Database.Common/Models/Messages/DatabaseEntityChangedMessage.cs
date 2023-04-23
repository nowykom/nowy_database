namespace Nowy.Database.Common.Models.Messages;

public enum DatabaseEntityChangedType : int
{
    NONE = 0,
    INSERT = 1,
    UPDATE = 2,
    DELETE = 3,
}

public class DatabaseEntityChangedMessage : IEquatable<DatabaseEntityChangedMessage>
{
    public static string GetName(DatabaseEntityChangedType type = 0, string? database_name = null, string? entity_name = null)
    {
        if (database_name is not null && entity_name is not null)
            return $"database:collection_changed:{(int)type}:{database_name}:{entity_name}";
        if (database_name is not null)
            return $"database:collection_changed:{(int)type}:{database_name}";
        return "database:collection_changed";
    }

    public DatabaseEntityChangedType Type { get; set; }
    public string? DatabaseName { get; set; }
    public string? EntityName { get; set; }
    public string? Id { get; set; }

    public bool Equals(DatabaseEntityChangedMessage? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return this.Type == other.Type && this.DatabaseName == other.DatabaseName && this.EntityName == other.EntityName && this.Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((DatabaseEntityChangedMessage)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.Type, this.DatabaseName, this.EntityName, this.Id);
    }
}
