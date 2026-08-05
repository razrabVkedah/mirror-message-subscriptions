using System;
using System.Collections.Generic;

namespace MirrorMessageSubscriptions.Networking
{
    public sealed class MessageSubscriptionRegistry
    {
        private static readonly IReadOnlyCollection<int> EmptySubscribers = Array.Empty<int>();
        private readonly Dictionary<ushort, HashSet<int>> _subscribersByMessageId = new();

        public void Add(int connectionId, ushort messageId)
        {
            if (!_subscribersByMessageId.TryGetValue(messageId, out HashSet<int> subscribers))
            {
                subscribers = new HashSet<int>();
                _subscribersByMessageId.Add(messageId, subscribers);
            }

            subscribers.Add(connectionId);
        }

        public IReadOnlyCollection<int> GetSubscribers(ushort messageId)
        {
            return _subscribersByMessageId.TryGetValue(messageId, out HashSet<int> subscribers)
                ? subscribers
                : EmptySubscribers;
        }

        public void RemoveConnection(int connectionId)
        {
            foreach (HashSet<int> subscribers in _subscribersByMessageId.Values)
            {
                subscribers.Remove(connectionId);
            }
        }

        public void Clear()
        {
            _subscribersByMessageId.Clear();
        }
    }
}
