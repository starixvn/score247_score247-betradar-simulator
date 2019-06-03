namespace LiveScore.Shared
{
    using System.Collections.Generic;
    using LiveScore.Shared.Models;

    public class Language : Enumeration
    {
        public static readonly Language English = new Language("en-US", nameof(English));
        public static readonly Language Vietnamese = new Language("vi-VN", nameof(Vietnamese));
        public static readonly Language Thailand = new Language("th-TH", nameof(Thailand));
        public static readonly Language Indonesia = new Language("id-ID", nameof(Indonesia));
        public static readonly Language TraditionalChinese = new Language("zh-TW", "Traditional Chinese");
        public static readonly Language SimplifiedChinese = new Language("zh-CN", "Simplified Chinese");

        public Language()
        {
        }

        private Language(string value, string displayName)
            : base(value, displayName)
        {
        }

        public static IEnumerable<Language> SupportedLanguages = new List<Language>
        {
            English,
            Vietnamese,
            Thailand,
            Indonesia,
            TraditionalChinese,
            SimplifiedChinese,
        };
    }
}
