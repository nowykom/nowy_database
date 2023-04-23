namespace Nowy.Database.Common.Models.Messages;

public class DatabaseCollectionChangedMessage : IEquatable<DatabaseCollectionChangedMessage>
{
    public static string GetName(DatabaseEntityChangedType type = 0, string? database_name = null, string? entity_name = null)
    {
        if (database_name is not null && entity_name is not null)
            return $"database:collection_changed:{(int)type}:{database_name}:{entity_name}";
        if (database_name is not null)
            return $"database:collection_changed:{(int)type}:{database_name}";
        return "database:collection_changed";
    }

    public string? DatabaseName { get; set; }
    public string? EntityName { get; set; }

    public bool Equals(DatabaseCollectionChangedMessage? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return this.DatabaseName == other.DatabaseName && this.EntityName == other.EntityName;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((DatabaseCollectionChangedMessage)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.DatabaseName, this.EntityName);
    }
}
