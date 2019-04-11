namespace LiveScore.Features.SportRadarApi
{
    public static class SportRadarApiConverter
    {
        private const int BasketballSportId = 2;
        private const int ESportsSportId = 3;

        public static string SportIdToName(int sportId)
        {
            switch (sportId)
            {
                case BasketballSportId:
                    return "basketball";

                case ESportsSportId:
                    return "sports";

                default:
                    return "soccer";
            }
        }
    }
}