using System;
using System.Collections.Generic;

namespace Framewerk.Managers
{
    public interface IFrameUpdater
    {
        void EveryFrame(Action action);
        void EveryNthFrame(int n, Action action);
        void RemoveFrameAction(Action action);
    }

    public class FrameUpdater : SingletonMono<FrameUpdater>, IFrameUpdater
    {
        private class FrameAction
        {
            public Action Action;
            public int FrameSampling;
            public Exception ReportedException;
        }

        private List<FrameAction> frameActions;
        private List<FrameAction> toAddActions;
        private List<Action> toRemoveActions;
        private long frameCounter;

        private bool reset = false;

        public FrameUpdater()
        {
            frameActions = new List<FrameAction>(20);
            frameCounter = 0;

            toAddActions = new List<FrameAction>(5);
            toRemoveActions = new List<Action>(5);
        }

        public void Reset()
        {
            reset = true;
        }

        void Update()
        {
            foreach (var action in toRemoveActions)
            {
                for (int i = frameActions.Count - 1; i >= 0; i--)
                {
                    if (frameActions[i].Action == action) frameActions.RemoveAt(i);
                }
            }
            toRemoveActions.Clear();

            foreach (var action in toAddActions)
            {
                frameActions.Add(action);
            }
            toAddActions.Clear();

            frameCounter++;
            foreach (var frameAction in frameActions)
            {
                FrameAction fr = frameAction;
                if (frameCounter % fr.FrameSampling == 0)
                {
                    try
                    {
                        if (fr.Action != null)
                            fr.Action();
                    }
                    catch (Exception ex)
                    {
                        if (fr.ReportedException == null || fr.ReportedException.Message != ex.Message)
                        {
                            fr.ReportedException = ex;
                            //TODO: log
                            //Logger.WriteError(ex, "FrameUpdater.Update", "Error when invoking action '" + fr.identifier + "' on update()!");
                        }
                    }

                }
                if(reset) continue;
            }

            if (reset)
            {
                frameActions.Clear();
                toAddActions.Clear();
                toRemoveActions.Clear();
                reset = false;
            }
        }

        public void EveryFrame(Action action)
        {
            EveryNthFrame(1, action);
        }

        public void EveryNthFrame(int n, Action action)
        {
            toAddActions.Add(new FrameAction() { Action = action, FrameSampling = n});
        }

        public void RemoveFrameAction(Action action)
        {
            toAddActions.RemoveAll(frameAction => frameAction.Action == action);
            toRemoveActions.Add(action);
        }

        protected override void SingletonMonoInit()
        {
            base.SingletonMonoInit();
            gameObject.name = "Framewerk.FrameUpdater";
        }
    }
}
