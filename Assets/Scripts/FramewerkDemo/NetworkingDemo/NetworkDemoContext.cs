using Framewerk;
using Framewerk.Networking;
using Framewerk.Networking.Discovery;
using Framewerk.Networking.StrangeIntegration;
using Framewerk.StrangeCore;
using FramewerkDemo.NetworkingDemo.Commands;
using FramewerkDemo.NetworkingDemo.Messages;
using FramewerkDemo.NetworkingDemo.Model;
using FramewerkDemo.NetworkingDemo.Signals;
using FramewerkDemo.NetworkingDemo.View;
using strange.extensions.context.impl;
using UnityEngine;

namespace FramewerkDemo.NetworkingDemo
{
    /// <summary>
    /// StrangeIoC Context for the Networking Demo.
    /// Maps all bindings including NetworkHost, NetworkDiscovery, NetworkCommandBinder,
    /// Views, Mediators, Signals, Commands, and the Model.
    /// </summary>
    public class NetworkDemoContext : FramewerkMVCSContext
    {
        public NetworkDemoContext(ContextView view) : base(view, true)
        {
        }

        protected override void mapBindings()
        {
            base.mapBindings();

            // NETWORKING CORE
            injectionBinder.Bind<NetworkHost>().ToSingleton();
            injectionBinder.Bind<NetworkDiscovery>().ToSingleton();
            injectionBinder.Bind<NetworkMessageReceivedSignal>().ToSingleton();

            // NetworkCommandBinder - maps messages to commands
            injectionBinder.Bind<INetworkCommandBinder>().To<NetworkCommandBinder>().ToSingleton();
            INetworkCommandBinder networkCommandBinder = injectionBinder.GetInstance<INetworkCommandBinder>();

            // Bind network messages to commands
            networkCommandBinder.Bind<PingMessage>().To<PingReceivedCommand>();
            networkCommandBinder.Bind<PongMessage>().To<PongReceivedCommand>();
            networkCommandBinder.Bind<ChatMessage>().To<ChatReceivedCommand>();

            // MODEL
            injectionBinder.Bind<INetworkDemoModel>().To<NetworkDemoModel>().ToSingleton();

            // SIGNALS
            injectionBinder.Bind<ChatLogUpdatedSignal>().ToSingleton();
            injectionBinder.Bind<DiscoveredHostsUpdatedSignal>().ToSingleton();
            injectionBinder.Bind<ConnectionStateChangedSignal>().ToSingleton();
            injectionBinder.Bind<RttUpdatedSignal>().ToSingleton();

            // VIEW
            mediationBinder.Bind<NetworkDemoView>().To<NetworkDemoMediator>();

            // COMMANDS
            commandBinder.Bind<ContextStartSignal>().To<NetworkDemoStartCommand>();

            // Create and inject NetworkDemoUpdater MonoBehaviour
            CreateUpdater();
        }

        private void CreateUpdater()
        {
            // Create a GameObject to host the NetworkDemoUpdater
            var updaterGo = new GameObject("NetworkDemoUpdater");
            var updater = updaterGo.AddComponent<NetworkDemoUpdater>();

            // Inject dependencies into the updater
            injectionBinder.injector.Inject(updater);

            // Make sure it persists and is under the context
            Object.DontDestroyOnLoad(updaterGo);
            updaterGo.transform.SetParent((contextView as GameObject).transform);
        }
    }
}
