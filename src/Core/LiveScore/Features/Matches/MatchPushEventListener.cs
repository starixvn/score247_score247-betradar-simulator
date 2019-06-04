namespace LiveScore.Features.Matches
{
    using System.Threading.Tasks;

    public interface IMatchPushEventListener
    {
        Task<bool> Start(byte sportId);
    }
}
