namespace LiveScore.Providers.SportRadar.Dtos
{
    using System;
    using System.Collections.Generic;

    public class RegionTournamentDto
    {
        public DateTime generated_at { get; set; }
        public string schema { get; set; }
        public IEnumerable<Tournament> tournaments { get; set; }
    }
}