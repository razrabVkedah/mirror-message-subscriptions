using MirrorMessageSubscriptions.Networking;
using Zenject;

namespace MirrorMessageSubscriptions.Installers
{
    public sealed class NetworkInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<MessageSubscriptionRegistry>().AsSingle();
            Container.Bind<INetworkMessageService>().To<MirrorMessageService>().AsSingle();
        }
    }
}
