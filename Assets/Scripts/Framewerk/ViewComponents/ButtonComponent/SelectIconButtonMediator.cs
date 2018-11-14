using System;
using System.Collections.Generic;
using Framewerk.Mvcs;
using UnityEngine;
using UnityEngine.Events;

namespace Framewerk.ViewComponents.ButtonComponent
{
    public class SelectIconButtonMediator : BaseSelectButtonMediator<IconButtonView, Sprite>
    {
        protected override void Init()
        {
            base.Init();
            SetItems(View.Items);
        }

        protected override void DisplayIndex()
        {
            View.SetIcon(SelectedItem);  
        }
    }
}