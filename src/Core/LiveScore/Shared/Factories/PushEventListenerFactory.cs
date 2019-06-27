namespace LiveScore.Shared
{
    using System;
    using System.Collections.Generic;
    using LiveScore.Features.Matches;

    public interface PushEventListenerFactory
    {
        IMatchPushEventListener CreatePushEventListener(int sportId);
    }

    public class PushEventListenerFactoryImpl : PushEventListenerFactory
    {
        private readonly IDictionary<int, IMatchPushEventListener> pushEventListenerContainer;

        public PushEventListenerFactoryImpl(
            IDictionary<int, IMatchPushEventListener> pushEventListenerContainer)
        {
            this.pushEventListenerContainer = pushEventListenerContainer;
        }

        public IMatchPushEventListener CreatePushEventListener(int sportId)
        {
            if (pushEventListenerContainer == null || !pushEventListenerContainer.ContainsKey(sportId))
            {
                throw new InvalidOperationException($"Cannot found implementation of PushEventListener for sport {sportId}");
            }

            return pushEventListenerContainer[sportId];
        }
    }
}