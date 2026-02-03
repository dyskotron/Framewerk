using Plugins.Framewerk;
using strange.extensions.command.impl;

namespace SomeSpace
{
    public class ExfoliatestStartCommand : Command
    {
        [Inject] public ViewConfig ViewConfig { get; set; }

        public override void Execute()
        {
            // Initialize your app here
        }
    }
}
