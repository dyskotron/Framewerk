using System;
using strange.extensions.command.api;
using strange.extensions.command.impl;
using strange.framework.api;

namespace Framewerk.Networking.StrangeIntegration
{
    public interface INetworkCommandBinder : ICommandBinder
    {

    }

    public class NetworkCommandBinder : CommandBinder, INetworkCommandBinder
    {
        [Inject] public NetworkMessageReceivedSignal NetworkMessageReceivedSignal { get; set; }

        private bool _signalListenerAdded = false;

        public override void ResolveBinding(IBinding binding, object key)
        {
            base.ResolveBinding(binding, key);

            if (!_signalListenerAdded)
            {
                NetworkMessageReceivedSignal.AddListener(NetworkMessageReceivedHandler);
                _signalListenerAdded = true;
            }
        }

        public override void OnRemove()
        {
            if (_signalListenerAdded)
                NetworkMessageReceivedSignal.RemoveListener(NetworkMessageReceivedHandler);
        }

        protected override ICommand invokeCommand(Type cmd, ICommandBinding binding, object data, int depth)
        {
            injectionBinder.Bind(data.GetType()).ToValue(data).ToInject(false);
            var command =  base.invokeCommand(cmd, binding, data, depth);
            injectionBinder.Unbind(data.GetType());

            return command;
        }

        private void NetworkMessageReceivedHandler(INetworkMessage message)
        {
            var key = message.GetType();
            if(!bindings.ContainsKey(key))
                return;

            ReactTo(message.GetType(), message);
        }
    }
}
