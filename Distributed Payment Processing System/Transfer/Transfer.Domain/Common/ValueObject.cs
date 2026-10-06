
namespace Transfer.Domain.Common
{
    public abstract class ValueObject
    {
        protected abstract IEnumerable<object?> GetEqualityComponents();
        public override bool Equals(object? obj)
        {
            return obj is ValueObject other&&
                GetType() == other.GetType()&&
                GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }
        public override int GetHashCode()
        {
            return GetEqualityComponents().Aggregate(0,HashCode.Combine);
        }
        public static bool operator ==(ValueObject? left, ValueObject? right) => object.Equals(left,right);
        public static bool operator !=(ValueObject? left, ValueObject? right) => !object.Equals(left, right);
    }
}
