namespace LiveScore.Shared
{
    using LiveScore.Shared.Models;

    public class Language: Enumeration
    {
        public const string En = "en-US";

        public static readonly Language CupRound = new Language(En, nameof(En));

        public Language()
        {
        }

        public Language(string value, string displayName)
            : base(value, displayName)
        {
        }
    }
}
