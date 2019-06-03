namespace LiveScore.Soccers.Jobs
{
    using LiveScore.Features.Matches;
    using System.Threading.Tasks;

    public interface IUpdateLiveMatchJob
    {
        Task Insert(string[] Ids);

        Task Delete(string[] Ids);
    }


    public class UpdateLiveMatchJob : IUpdateLiveMatchJob
    {
        private readonly IMatchService matchService;

        public UpdateLiveMatchJob(
             IMatchService matchService)
        {
            this.matchService = matchService;
        }

        public Task Delete(string[] Ids)
        {
            throw new System.NotImplementedException();
        }

        public Task Insert(string[] Ids)
        {
            throw new System.NotImplementedException();
        }

        //private async Task UpdateLiveMatches(List<string> matchIds, string language)
        //{
        //    // status == live && not exist in live table -> insert to live table -> update cache

        //    foreach (var id in matchIds)
        //    {
        //        var liveMatchEntity = await repository.GetById<LiveMatchEntity>(id);

        //        if (liveMatchEntity == null)
        //        {
        //            var matchEntity = await repository.GetById<MatchEntity>(id);

        //            liveMatchEntity = new LiveMatchEntity
        //            {
        //                Id = matchEntity.Id,
        //                SportId = matchEntity.SportId,
        //                Language = matchEntity.Language,
        //                Value = matchEntity.Value,
        //                LeagueId = matchEntity.LeagueId,
        //                EventDate = matchEntity.EventDate.ToUniversalTime()
        //            };

        //            await repository.Insert(liveMatchEntity);
        //        }
        //    }

        //    await repository.Save();

        //}

        //private async Task UpdateClosedMatches(List<string> matchIds, string language)
        //{
        //    // status == closed && exist in live table -> delete in live table ->  update cache

        //    foreach (var id in matchIds)
        //    {
        //        //TODO update get id pair
        //        var liveMatchEntity = await repository.GetById<LiveMatchEntity>(id);

        //        if (liveMatchEntity != null)
        //        {
        //            repository.Delete<LiveMatchEntity>(liveMatchEntity);

        //            //TODO update result of matches?
        //            var matchEntity = await repository.GetById<MatchEntity>(id);

        //        }
        //    }



        //    await repository.Save();
        //}
    }
}
