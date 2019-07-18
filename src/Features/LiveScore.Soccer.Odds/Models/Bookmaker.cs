namespace LiveScore.Soccer.Odds.Models
{
    using LiveScore.Soccer.Models;

    public class Bookmaker : Entity<string, string>
    {
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var actualObj = obj as Bookmaker;
            if (actualObj == null)
            {
                return false;
            }

            return Id == actualObj.Id
                && Name == actualObj.Name;
        }
    }
}