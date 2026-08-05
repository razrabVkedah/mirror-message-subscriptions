using System;
using Mirror;

namespace MirrorMessageSubscriptions.Networking
{
    public interface INetworkMessageService
    {
        void InitializeServer();
        void ShutdownServer();
        void Subscribe<T>(Action<T> handler) where T : struct, NetworkMessage;
        void SendToSubscribers<T>(T message) where T : struct, NetworkMessage;
        void RemoveConnection(int connectionId);
    }
}
