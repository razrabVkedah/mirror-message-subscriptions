using System;
using System.Collections.Generic;
using Mirror;
using MirrorMessageSubscriptions.Networking.Messages;
using UnityEngine;

namespace MirrorMessageSubscriptions.Networking
{
    public sealed class MirrorMessageService : INetworkMessageService
    {
        private readonly MessageSubscriptionRegistry _registry;
        private readonly HashSet<ushort> _registeredClientMessageIds = new();
        private bool _isServerInitialized;

        public MirrorMessageService(MessageSubscriptionRegistry registry)
        {
            _registry = registry;
        }

        public void InitializeServer()
        {
            if (_isServerInitialized)
            {
                return;
            }

            NetworkServer.RegisterHandler<MessageSubscriptionRequest>(OnSubscriptionRequested);
            _isServerInitialized = true;
        }

        public void ShutdownServer()
        {
            if (_isServerInitialized)
            {
                NetworkServer.UnregisterHandler<MessageSubscriptionRequest>();
                _isServerInitialized = false;
            }

            _registry.Clear();
        }

        public void Subscribe<T>(Action<T> handler) where T : struct, NetworkMessage
        {
            ushort messageId = NetworkMessages.GetId<T>();
            if (_registeredClientMessageIds.Add(messageId))
            {
                NetworkClient.RegisterHandler(handler);
            }

            if (!NetworkClient.isConnected)
            {
                Debug.LogWarning($"Cannot subscribe to {typeof(T).Name}: the client is not connected.");
                return;
            }

            NetworkClient.Send(new MessageSubscriptionRequest { MessageId = messageId });
            Debug.Log($"Subscribed to {typeof(T).Name} (message id: {messageId}).");
        }

        public void SendToSubscribers<T>(T message) where T : struct, NetworkMessage
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning($"Cannot send {typeof(T).Name}: the server is not active.");
                return;
            }

            foreach (int connectionId in _registry.GetSubscribers(NetworkMessages.GetId<T>()))
            {
                if (NetworkServer.connections.TryGetValue(connectionId, out NetworkConnectionToClient connection))
                {
                    connection.Send(message);
                }
            }
        }

        public void RemoveConnection(int connectionId)
        {
            _registry.RemoveConnection(connectionId);
        }

        private void OnSubscriptionRequested(NetworkConnectionToClient connection, MessageSubscriptionRequest request)
        {
            _registry.Add(connection.connectionId, request.MessageId);
            Debug.Log($"Connection {connection.connectionId} subscribed to message id {request.MessageId}.");
        }
    }
}
