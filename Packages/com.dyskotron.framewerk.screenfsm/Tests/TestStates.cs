using System;
using System.Collections.Generic;

namespace Framewerk.AppStateMachine.Tests
{
    /// <summary>
    /// Records the calls of the test states and screens, in order.
    /// </summary>
    public class CallLog
    {
        public readonly List<string> Calls = new List<string>();

        public void Add(string call)
        {
            Calls.Add(call);
        }
    }

    public class TestFsmScreen : FsmStateScreen
    {
        public CallLog Log;
        public int DestroyCount;

        protected override void Enter()
        {
            Log.Add("screen.Enter");
        }

        protected override void Exit()
        {
            Log.Add("screen.Exit");
        }

        public override void SubstateIn(IAppState state)
        {
            Log.Add($"screen.SubstateIn {NameOf(state)}");
        }

        public override void SubstateOut(IAppState state)
        {
            Log.Add($"screen.SubstateOut {NameOf(state)}");
        }

        public override void Destroy()
        {
            DestroyCount++;
            Log.Add("screen.Destroy");
            base.Destroy();
        }

        private static string NameOf(IAppState state)
        {
            return state is TestSubState subState ? subState.Name : state.GetType().Name;
        }
    }

    public class TestPlainScreen : AppStateScreen
    {
        public CallLog Log;

        protected override void Enter()
        {
            Log.Add("plainScreen.Enter");
        }

        protected override void Exit()
        {
            Log.Add("plainScreen.Exit");
        }

        public override void Destroy()
        {
            Log.Add("plainScreen.Destroy");
            base.Destroy();
        }
    }

    public class TestParentState : FsmState<TestFsmScreen>
    {
        public CallLog Log;

        /// <summary>
        /// Substate switched in once the screen has entered, before the parent reports its enter finished.
        /// </summary>
        public Func<IAppState> FirstSubstate;

        protected override void Enter()
        {
            Log.Add("parent.Enter");
        }

        protected override void Exit()
        {
            Log.Add("parent.Exit");
        }

        protected override void EnterFinished()
        {
            if (FirstSubstate != null)
                SwitchState(FirstSubstate());

            base.EnterFinished();
        }

        protected override void RegisterHandlers()
        {
            Log.Add("parent.RegisterHandlers");
        }

        protected override void UnregisterHandlers()
        {
            Log.Add("parent.UnregisterHandlers");
        }
    }

    public class TestSubState : SubState<TestFsmScreen>
    {
        public string Name;
        public CallLog Log;

        /// <summary>
        /// Called in Enter, while the substate's enter transition runs.
        /// </summary>
        public Action OnEnter;

        /// <summary>
        /// Holds the enter transition open until <see cref="ReleaseEnter"/>.
        /// </summary>
        public bool HoldEnter;

        protected override void Enter()
        {
            Log.Add($"{Name}.Enter");
            OnEnter?.Invoke();
        }

        protected override void Exit()
        {
            Log.Add($"{Name}.Exit");
        }

        protected override void EnterFinished()
        {
            if (HoldEnter)
                return;

            base.EnterFinished();
        }

        public void ReleaseEnter()
        {
            HoldEnter = false;
            base.EnterFinished();
        }

        public override void Destroy()
        {
            Log.Add($"{Name}.Destroy");
            base.Destroy();
        }
    }

    /// <summary>
    /// A state with a screen of its own, run by the app's machine (or, wrongly, as a substate).
    /// </summary>
    public class TestPlainState : AppState<TestPlainScreen>
    {
        public string Name;
        public CallLog Log;
        public Action OnEnter;

        protected override void Enter()
        {
            Log.Add($"{Name}.Enter");
            OnEnter?.Invoke();
        }

        protected override void Exit()
        {
            Log.Add($"{Name}.Exit");
        }

        public override void Destroy()
        {
            Log.Add($"{Name}.Destroy");
            base.Destroy();
        }
    }

    /// <summary>
    /// A state that isn't a SubState but shares the parent's screen type.
    /// </summary>
    public class TestNonSubState : AppState<TestFsmScreen>
    {
    }
}
