using strange.extensions.mediation.impl;
using UnityEngine;

namespace Framewerk.Examples.TabsViewStackExample
{
    /// <summary>
    /// Mediator for ContentPanelView.
    /// Receives ContentPanelData via injection and sets the view's text fields.
    /// </summary>
    public class ContentPanelMediator : Mediator
    {
        [Inject]
        public ContentPanelView View { get; set; }
        
        [Inject]
        public ContentPanelData Data { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();
            
            if (Data == null)
            {
                Debug.LogWarning("[ContentPanelMediator] No ContentPanelData injected");
                return;
            }

            // Set the panel name for debugging
            View.gameObject.name = $"{Data.Title}Panel";
            
            // Set title
            if (View.Title != null)
            {
                View.Title.text = Data.Title;
            }
            
            // Set description
            if (View.Description != null)
            {
                View.Description.text = Data.Description;
            }
            
            // Set background color
            if (View.Background != null)
            {
                View.Background.color = Data.BackgroundColor;
            }
        }
    }
}
