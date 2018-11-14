using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Framewerk.ViewComponents.ButtonComponent
{
    public class IconButtonView : ButtonView
    {
        public Image Icon;
        public List<Sprite> Items;
        
        public void SetIcon(Sprite icon)
        {
            if(icon != null)
            Icon.sprite = icon;
        }
    }
}