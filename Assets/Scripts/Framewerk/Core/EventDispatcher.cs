using System;
using System.Collections;
using System.Collections.Generic;
using Framewerk.Events;
using UnityEngine;

namespace Framewerk.Core
{
    public interface IEventDispatcher
    {
        void AddListener<T> (Action<T> a) where T : AbstractEvent;

        bool HasListener<T> (Action<T> a) where T : AbstractEvent;

        void RemoveListener<T> (Action<T> a) where T : AbstractEvent;

        void DispatchEvent <T> (T e) where T : AbstractEvent;

        void RemoveAll();

        void LogAllListeners();
    }

    public class EventDispatcher : IEventDispatcher
    {
        /*
	     * EVENTS
	     */
        private Dictionary<Type, IList> events;

        public EventDispatcher ()
        {
            events = new Dictionary<Type, IList> ();
        }

        public void RemoveAll()
        {
            events = new Dictionary<Type, IList>();
        }

        public void AddListener<T> (Action<T> a) where T : AbstractEvent
        {
            Type t = typeof(T);
            if (events.ContainsKey (t)) {
                IList l = events [t];
                if (l.Contains (a)) {
                    Debug.LogWarningFormat("<color=\"aqua\">{0}.AddListener : OBSERVER WARNING: Event {1} is already registerd with action {2}</color>", this, t, a); 
                }
                else
                {
                    l.Add (a);
                }
            } else {
                events.Add (t, new List<Action<T>>{ a });
            }
        }

        public bool HasListener<T> (Action<T> a) where T : AbstractEvent
        {
            Type t = typeof(T);
            if (events.ContainsKey (t)) {
                IList l = events [t];
                return l.Contains (a);
            }

            return false;
        }

        public void RemoveListener<T> (Action<T> a) where T : AbstractEvent
        {
            Type t = typeof(T);
            if (events.ContainsKey (t)) {
                IList l = events [t];
                l.Remove (a);
            }
        }

        public void DispatchEvent <T> (T e) where T : AbstractEvent
        {
            Type t = typeof(T);
            // TODO: možná by šlo Type t = e.GetType();
            if (events.ContainsKey (t)) {
                IList l = events [t];
                for (int i = l.Count-1; i > -1 ; i--) {
                    Action<T> a = (Action<T>)l [i];
                    a.Invoke(e);
                    /*
                    try
                    {
                        a.Invoke(e);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogErrorFormat("<color=\"red\">{0}.DispatchEvent :Error when reacting to event {1}</color>\n {2}", this, e.GetType().Name, e.ToString());
                    }*/
                }
            }
        }

        public void LogAllListeners()
        {
            foreach (KeyValuePair<Type, IList> listeners in events)
            {
                var color = listeners.Value.Count > 1 ? "red" : "magenta";
                if(listeners.Value.Count > 0)
                    Debug.LogWarningFormat("EventType: <color=\"{2}\">{0}</color> : Num Listeners:<color=\"{2}\">{1}</color>", listeners.Key, listeners.Value.Count, color);    
            }
            
        }
    }
}

