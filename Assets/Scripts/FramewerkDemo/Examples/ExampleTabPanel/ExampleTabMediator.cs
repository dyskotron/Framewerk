using System.Collections.Generic;
using Framewerk.ViewComponents.TabComponent;
using UnityEngine;

namespace FramewerkDemo.Examples.ExampleTabPanel
{
    public class ExampleTabMediator : TabMediator<ExampleTabView>
    {
        private List<Color> _colors;

        public ExampleTabMediator()
        {
            _colors = new List<Color>();
            _colors.Add(Color.yellow);
            _colors.Add(Color.cyan);
            _colors.Add(Color.white);
            _colors.Add(Color.blue);
        }
        
        public void SetTabIndex(int tabIndex)
        {
            View.Message.text = string.Format("This is tab no {0}", tabIndex + 1);
            //View.BackgroundImage.color = _colors[Math.Min(_colors.Count, tabIndex)];
        }
    }
}