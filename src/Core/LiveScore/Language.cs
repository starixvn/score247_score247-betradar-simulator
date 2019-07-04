namespace LiveScore.Soccer
{
    using LiveScore.Soccer.Models;

    public class Language : Enumeration
    {

        public static readonly Language En = new Language("en-US", nameof(En));

        public Language()
        {
        }

        private Language(string value, string displayName)
            : base(value, displayName)
        {
        }
    }
}
