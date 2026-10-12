namespace Framewerk.UI.List
{
    /// <summary>
    /// What user input changes a list's selection. Programmatic selection (SelectItemAt etc.) always works.
    /// </summary>
    public enum SelectionTrigger
    {
        /// <summary>Clicking an item selects it (default).</summary>
        Click,

        /// <summary>Pointer enter selects the item; clicks only fire ListItemClicked / ItemClickedSignal.
        /// Requires ListItemView.PointerElement on the items.</summary>
        Hover,

        /// <summary>Only code changes selection; clicks only fire ListItemClicked / ItemClickedSignal.</summary>
        None
    }
}
