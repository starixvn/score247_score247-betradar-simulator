namespace LiveScore.BetRadarSimulator.Api.Features.Matches
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Newtonsoft.Json;


    public interface IMatchService
    {
        Task<MatchTimelineDto> GetTimeline(string matchId);
    }

    public class MatchService : IMatchService
    {
        public async Task<MatchTimelineDto> GetTimeline(string matchId)
        {
            var path = $"{AppDomain.CurrentDomain.BaseDirectory}/App_Data/staticdata/match-timelines.json";
            var jsonData = await File.ReadAllTextAsync(path);
            var timelineDto = JsonConvert.DeserializeObject<MatchTimelineDto>(jsonData);

            timelineDto.generated_at = DateTime.Now;
            timelineDto.sport_event.id = matchId;

            return timelineDto;
        }
    }
}
