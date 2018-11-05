using System;
using Boo.Lang;
using Framewerk.ViewComponents.ListComponent;
using FramewerkDemo.Examples.ExampleListPanel;
using Random = UnityEngine.Random;

namespace FramewerkDemo.Examples.ExampleVirtualListPanel
{
    public class ExampleVirtualListPanelMediator : VirtualListMediator<ListContainerView, ExampleVirtualLisItemMediator, ExampleListDataProvider>
    {
        protected override void Init()
        {
            base.Init();
            
            //create dummy data
            var names = new List<String>();
            names.Add("Daisy");
            names.Add("Kitty");
            names.Add("Gracie");
            names.Add("Jack");
            names.Add("Nala");
            names.Add("Smokey");
            names.Add("Callie");
            names.Add("Smokey");
            names.Add("Simba");
            names.Add("Milo");
            names.Add("Chloe");

            var _data = new System.Collections.Generic.List<ExampleListDataProvider>();
            for (var i = 0; i < 5000; i++)
            {
                _data.Add(new ExampleListDataProvider(names[Random.Range(0, names.Count - 1)], Random.value > 0.5f));
            }
            
            SetData(_data);
        }
    }
}