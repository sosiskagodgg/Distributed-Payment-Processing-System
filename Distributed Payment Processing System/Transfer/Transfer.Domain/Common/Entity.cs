namespace Transfer.Domain.Common;

public abstract class Entity<TId> where TId : notnull 
{
    public TId Id { get; protected init; }

    protected Entity(TId id)
    {
        Id = id;
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> other&&
            GetType() == other.GetType()&&
            EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, GetType());
    }
    public static bool operator == (Entity<TId>? left, Entity<TId>? right)=> object.Equals(left, right);
    public static bool operator != (Entity<TId>? left, Entity<TId>? right) => !object.Equals(left, right);
}

