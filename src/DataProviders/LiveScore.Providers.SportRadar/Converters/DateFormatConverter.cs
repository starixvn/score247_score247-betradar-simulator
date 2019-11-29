using Newtonsoft.Json.Converters;

namespace LiveScore.Providers.SportRadar.Converters
{
    public class DateFormatConverter : IsoDateTimeConverter
    {
        public DateFormatConverter(string format)
        {
            DateTimeFormat = format;
        }
    }
}