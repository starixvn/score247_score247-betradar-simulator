namespace LiveScore.Shared
{
    using System;
    using System.Collections.Generic;
    using LiveScore.Features.Matches;

    public interface PushHandlerFactory
    {
        IMatchPushEventHandler CreateMatchPushEventHandler(int sportId);
    }

    public class PushHandlerFactoryImpl : PushHandlerFactory
    {
        private readonly IDictionary<int, IMatchPushEventHandler> matchPushEventHandlerContainer;

        public PushHandlerFactoryImpl(IDictionary<int, IMatchPushEventHandler> matchPushEventHandlerContainer)
        {
            this.matchPushEventHandlerContainer = matchPushEventHandlerContainer;
        }

        public IMatchPushEventHandler CreateMatchPushEventHandler(int sportId)
        {
            if (matchPushEventHandlerContainer == null || !matchPushEventHandlerContainer.ContainsKey(sportId))
            {
                throw new InvalidOperationException($"Cannot found implementation of Match Push Event Handler for sport {sportId}");
            }

            return matchPushEventHandlerContainer[sportId];
        }
    }
}
