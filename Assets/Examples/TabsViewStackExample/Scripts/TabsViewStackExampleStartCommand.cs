using System.Threading;
using Framewerk.Managers;
using strange.extensions.command.impl;

namespace Framewerk.Examples.TabsViewStackExample
{
    /// <summary>
    /// Start command demonstrating the proper ViewGroup pattern.
    /// 
    /// The ViewGroup pattern instantiates MULTIPLE views together, sharing state via
    /// [ViewGroupShared] injection. The framework automatically creates and shares
    /// a SelectionChangedSignal instance between all mediators in the group that
    /// have [Inject, ViewGroupShared] on the same type.
    /// 
    /// Key principles:
    /// 1. Views are SEPARATE - each prefab contains ONE view type
    /// 2. Views are instantiated TOGETHER via InstantiateViewsAsync(ViewGroup)
    /// 3. Shared state is automatic - no manual signal creation/passing needed
    /// 4. Each mediator handles its OWN concern (separation of concerns)
    /// 
    /// Pattern: Command spawns container → Container mediator spawns ViewGroup
    /// Everything goes through UiManager - no manual GameObject creation.
    /// </summary>
    public class TabsViewStackExampleStartCommand : Command
    {
        [Inject] public IUiManager UiManager { get; set; }

        public override void Execute()
        {
            // Just instantiate the container - its mediator handles the rest
            _ = UiManager.InstantiateViewAsync<TabsViewStackContainerView>(ct: CancellationToken.None);
        }
    }
}
