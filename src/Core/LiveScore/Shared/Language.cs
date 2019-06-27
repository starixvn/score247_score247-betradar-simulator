namespace LiveScore.Shared
{
    using LiveScore.Shared.Models;

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
