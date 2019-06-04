namespace LiveScore.Shared.Factories
{
    using LiveScore.Features.Matches;
    using System;
    using System.Collections.Generic;

    public interface PushListenerFactory
    {
        IMatchPushEventListener CreatePushEventListener(int sportId);
    }

    public class PushListenerFactoryImpl : PushListenerFactory
    {
        private readonly IDictionary<int, IMatchPushEventListener> matchPushEventListenerContainer;

        public PushListenerFactoryImpl(IDictionary<int, IMatchPushEventListener> matchPushEventListenerContainer)
        {
            this.matchPushEventListenerContainer = matchPushEventListenerContainer;
        }

        public IMatchPushEventListener CreatePushEventListener(int sportId)
        {
            if (matchPushEventListenerContainer == null || !matchPushEventListenerContainer.ContainsKey(sportId))
            {
                throw new InvalidOperationException($"Cannot found implementation of Match Event Listener for sport {sportId}");
            }

            return matchPushEventListenerContainer[sportId];
        }
    }
}
