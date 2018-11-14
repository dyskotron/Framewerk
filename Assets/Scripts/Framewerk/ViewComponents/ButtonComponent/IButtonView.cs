using UnityEngine.UI;

namespace Framewerk.ViewComponents.ButtonComponent
{
    public interface IButtonView
    {
        bool Interactable { get; set; }
        void SetLabel(string text);
        Button Button { get;}
    }
}