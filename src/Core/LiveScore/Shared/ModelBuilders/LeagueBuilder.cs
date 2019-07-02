namespace LiveScore.Shared.ModelBuilders
{
    using LiveScore.Features.Leagues.Models;
    using LiveScore.Shared.Models;
    using LiveScore.Soccer.Dtos;

    public static class LeagueBuilder
    {
        public static LeagueRound BuildLeagueRound(Tournament_Round leagueRoundDto)
        {
            var leagueRound = new LeagueRound();

            if (leagueRoundDto != null)
            {
                leagueRound.Type = Enumeration.FromValue<LeagueRoundTypes>(leagueRoundDto.type);
                leagueRound.Name = leagueRoundDto.name;
                leagueRound.Number = leagueRoundDto.number;
                leagueRound.Phase = leagueRoundDto.phase;
                leagueRound.Group = leagueRoundDto.group;
            }

            return leagueRound;
        }

        public static League BuildLeague(Tournament leagueDto)
            => new League
            {
                Id = leagueDto?.id,
                Name = leagueDto?.name,
                Category = new LeagueCategory
                {
                    Id = leagueDto?.category?.id,
                    Name = leagueDto?.category?.name,
                    CountryCode = leagueDto?.category?.country_code
                }
            };
    }
}