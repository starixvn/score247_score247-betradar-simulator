namespace LiveScore.Jobs.Features.Matches
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LiveScore.Entity.Entities;
    using LiveScore.Shared.SportRadarApi.Models;

    public interface MatchJobDataAccess
    {
        Task<IEnumerable<MatchEntity>> GetDailySchedule(int sportId, Region region, DateTime date, string language);
    }
}