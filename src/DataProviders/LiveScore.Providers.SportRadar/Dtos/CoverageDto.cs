namespace LiveScore.Providers.SportRadar.Dtos
{
    public class CoverageDto
    {
        public bool basic_score { get; set; }
        public bool key_events { get; set; }
        public bool detailed_events { get; set; }
        public bool lineups { get; set; }
        public bool commentary { get; set; }
    }

    public class CoverageInfoDto
    {
        public string level { get; set; }
        public bool live_coverage { get; set; }
        public CoverageDto coverage { get; set; }
    }
}
