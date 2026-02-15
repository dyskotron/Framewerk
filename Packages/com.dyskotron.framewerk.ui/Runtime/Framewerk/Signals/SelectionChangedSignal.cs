using strange.extensions.signal.impl;

namespace Framewerk.Signals
{
    /// <summary>
    /// Signal dispatched when selection changes in a list or tab component.
    /// The int? parameter is the selected index, or null if nothing is selected.
    /// </summary>
    public class SelectionChangedSignal : Signal<int?>
    {
    }
}
