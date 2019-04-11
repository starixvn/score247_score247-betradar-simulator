namespace LiveScore.Soccers.Features.Matches.DataProviders
{
    using System;
    using System.Threading.Tasks;
    using LiveScore.Shared.Models.Dtos;

    public interface IMatchApi
    {
        Task<DailyScheduleDto> GetDailySchedules(
            int sportId,
            DateTime date,
            string language);
    }
}