using Mirror;
using MirrorMessageSubscriptions.Networking;
using MirrorMessageSubscriptions.Networking.Messages;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using kcp2k;

namespace MirrorMessageSubscriptions.Demo
{
    public sealed class NetworkConnectionUi : MonoBehaviour
    {
        [SerializeField] private InputField _addressInput;
        [SerializeField] private Text _statusText;
        [SerializeField] private Text _lastMessageText;
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _clientButton;
        [SerializeField] private Button _stopButton;
        [SerializeField] private Button _subscribeButton;
        [SerializeField] private Button _sendButton;

        private TestNetworkManager _networkManager;
        private INetworkMessageService _messageService;
        private string _errorMessage;

        [Inject]
        private void Construct(INetworkMessageService messageService)
        {
            _messageService = messageService;
        }

        private void Awake()
        {
            _networkManager = GetComponent<TestNetworkManager>();

            _addressInput.text = $"{_networkManager.networkAddress}:{GetTransport().Port}";
            _networkManager.ServerError += OnServerError;
            _hostButton.onClick.AddListener(StartHost);
            _clientButton.onClick.AddListener(StartClient);
            _stopButton.onClick.AddListener(StopNetwork);
            _subscribeButton.onClick.AddListener(Subscribe);
            _sendButton.onClick.AddListener(SendHelloMessage);
        }

        private void Update()
        {
            bool isActive = NetworkServer.active || NetworkClient.active;
            _hostButton.interactable = !isActive;
            _clientButton.interactable = !isActive;
            _stopButton.interactable = isActive;
            _subscribeButton.interactable = NetworkClient.isConnected;
            _sendButton.interactable = NetworkServer.active;
            _subscribeButton.gameObject.SetActive(NetworkClient.isConnected);
            _sendButton.gameObject.SetActive(NetworkServer.active);

            _statusText.text = !string.IsNullOrEmpty(_errorMessage)
                ? _errorMessage
                : NetworkServer.active && NetworkClient.isConnected
                ? "Host запущен"
                : NetworkServer.active
                    ? "Сервер запущен"
                    : NetworkClient.isConnected
                        ? "Клиент подключён"
                        : NetworkClient.active
                            ? "Подключение..."
                            : "Нет подключения";
        }

        private void StartHost()
        {
            _errorMessage = null;
            ApplyConnectionSettings();
            _networkManager.StartHost();
        }

        private void StartClient()
        {
            _errorMessage = null;
            ApplyConnectionSettings();
            _networkManager.StartClient();
        }

        private void ApplyConnectionSettings()
        {
            string[] addressParts = _addressInput.text.Split(':');
            _networkManager.networkAddress = addressParts[0];

            if (addressParts.Length > 1 && ushort.TryParse(addressParts[1], out ushort port))
            {
                GetTransport().Port = port;
            }
        }

        private KcpTransport GetTransport()
        {
            return GetComponent<KcpTransport>();
        }

        private void StopNetwork()
        {
            if (NetworkServer.active && NetworkClient.active)
                _networkManager.StopHost();
            else if (NetworkServer.active)
                _networkManager.StopServer();
            else
                _networkManager.StopClient();
        }

        private void Subscribe()
        {
            _messageService.Subscribe<HelloMessage>(OnHelloMessage);
        }

        private void SendHelloMessage()
        {
            _messageService.SendToSubscribers(new HelloMessage { Text = "Hello Client!" });
        }

        private void OnHelloMessage(HelloMessage message)
        {
            _lastMessageText.text = "Последнее сообщение: " + message.Text;
        }

        private void OnServerError(string message)
        {
            _errorMessage = message;
        }
    }
}
