using Mirror;

namespace MirrorMessageSubscriptions.Networking.Messages
{
    public struct MessageSubscriptionRequest : NetworkMessage
    {
        public ushort MessageId;
    }
}
