namespace LiveScore.Features.Matches
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LiveScore.Features.Matches.Models;

    public interface IMatchPushEventHandler
    {
        Task OnReceive(string eventData);

        Task Handle(Dictionary<string, MatchPushEvent> pushEvents);
    }
}
