namespace LiveScore.Shared.Models
{
    public interface IEntity<out TId, out TName>
    {
        TId Id { get; }

        TName Name { get; }
    }

#pragma warning disable S1694 // An abstract class should have both abstract and concrete methods

    public abstract class Entity<TId, TName>
#pragma warning restore S1694 // An abstract class should have both abstract and concrete methods
    {
        public TId Id { get; set; }

        public TName Name { get; set; }
    }
}