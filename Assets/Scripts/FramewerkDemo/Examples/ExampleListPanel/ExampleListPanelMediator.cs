using System;
using System.Collections.Generic;
using Framewerk.ViewComponents.ListComponent;
using Random = UnityEngine.Random;

namespace FramewerkDemo.Examples.ExampleListPanel
{
    public class ExampleListPanelMediator : ListContainerMediator<ListContainerView, ExampleLisItemMediator, ExampleListDataProvider>
    {
        protected override void Init()
        {
            base.Init();

            //create Dummy data
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

            var _data = new List<ExampleListDataProvider>();
            for (var i = 0; i < 20; i++)
            {
                _data.Add(new ExampleListDataProvider(names[Random.Range(0, names.Count - 1)], Random.value > 0.5f));
            }

            SetData(_data);
        }
    }
}