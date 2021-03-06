﻿using System;
using System.Collections.Generic;
using Framewerk.Mvcs;
using UnityEngine.Events;

namespace Framewerk.ViewComponents.ButtonComponent
{
     public class SelectButtonMediator : BaseSelectButtonMediator<SelectButtonView, string>
    {
        protected override void DisplayIndex()
        {
            View.SetLabel(SelectedItem);    
        }
    }
}