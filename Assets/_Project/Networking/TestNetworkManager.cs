using System;
using Mirror;
using Zenject;

namespace MirrorMessageSubscriptions.Networking
{
    public sealed class TestNetworkManager : NetworkManager
    {
        public event Action<string> ServerError;

        private INetworkMessageService _messageService;

        [Inject]
        private void Construct(INetworkMessageService messageService)
        {
            _messageService = messageService;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            _messageService.InitializeServer();
        }

        public override void OnStopServer()
        {
            _messageService.ShutdownServer();
            base.OnStopServer();
        }

        public override void OnServerDisconnect(NetworkConnectionToClient connection)
        {
            _messageService.RemoveConnection(connection.connectionId);
            base.OnServerDisconnect(connection);
        }

        public override void OnServerError(NetworkConnectionToClient connection, TransportError error, string reason)
        {
            ServerError?.Invoke($"Server error: {reason}");
            base.OnServerError(connection, error, reason);
        }
    }
}
