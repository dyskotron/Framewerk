using System.Text.RegularExpressions;
using NUnit.Framework;
using strange.extensions.injector.api;
using strange.extensions.injector.impl;
using strange.framework.api;
using UnityEngine;
using UnityEngine.TestTools;

namespace Framewerk.AppStateMachine.Tests
{
    /// <summary>
    /// FsmState, SubState and FsmStateScreen: a parent state run by the app's machine, its substates sharing its screen.
    /// </summary>
    [TestFixture]
    public class TestFsmState
    {
        private IInjectionBinder _injectionBinder;
        private CallLog _log;
        private IAppFsm _appFsm;
        private TestFsmScreen _screen;

        [SetUp]
        public void SetUp()
        {
            _log = new CallLog();
            _screen = new TestFsmScreen { Log = _log };

            _injectionBinder = new InjectionBinder();
            _injectionBinder.Bind<IInjectionBinder>().Bind<IInstanceProvider>().ToValue(_injectionBinder);
            _injectionBinder.Bind<IAppFsm>().To<AppFsm>().ToSingleton();
            _injectionBinder.Bind<AppStateEnterSignal>().ToSingleton();
            _injectionBinder.Bind<AppStateExitSignal>().ToSingleton();
            _injectionBinder.Bind<TestFsmScreen>().ToValue(_screen).ToInject(false);
            _injectionBinder.Bind<TestPlainScreen>().ToValue(new TestPlainScreen { Log = _log }).ToInject(false);

            _injectionBinder.GetInstance<AppStateEnterSignal>().AddListener(type => _log.Add($"AppStateEnter {type.Name}"));
            _injectionBinder.GetInstance<AppStateExitSignal>().AddListener(type => _log.Add($"AppStateExit {type.Name}"));

            _appFsm = _injectionBinder.GetInstance<IAppFsm>();
        }

        private TestParentState Parent(TestSubState firstSubstate = null)
        {
            var parent = new TestParentState { Log = _log };
            if (firstSubstate != null)
                parent.FirstSubstate = () => firstSubstate;
            return parent;
        }

        private TestSubState Sub(string name)
        {
            return new TestSubState { Name = name, Log = _log };
        }

        [Test]
        public void Enter_ScreenEntersBeforeTheFirstSubstate_ParentReportsEnteredWithItIn()
        {
            var a = Sub("A");
            var parent = Parent(a);

            _appFsm.SwitchState(parent);

            CollectionAssert.AreEqual(new[]
            {
                "parent.RegisterHandlers",
                "parent.Enter",
                "screen.Enter",
                "A.Enter",
                "screen.SubstateIn A",
                "AppStateEnter TestParentState",
            }, _log.Calls);
            Assert.AreSame(a, parent.CurrentState);
            Assert.AreEqual(TransitionType.None, parent.CurrentTransition);
        }

        [Test]
        public void Enter_WithoutSubstate_EntersParentOnly()
        {
            var parent = Parent();

            _appFsm.SwitchState(parent);

            CollectionAssert.AreEqual(new[]
            {
                "parent.RegisterHandlers",
                "parent.Enter",
                "screen.Enter",
                "AppStateEnter TestParentState",
            }, _log.Calls);
            Assert.IsNull(parent.CurrentState);
        }

        [Test]
        public void SwitchState_ExitsOldSubstateThenEntersNew_OnTheSharedScreen()
        {
            var a = Sub("A");
            var parent = Parent(a);
            _appFsm.SwitchState(parent);
            _log.Calls.Clear();

            var b = Sub("B");
            parent.SwitchState(b);

            CollectionAssert.AreEqual(new[]
            {
                "A.Exit",
                "screen.SubstateOut A",
                "A.Destroy",
                "B.Enter",
                "screen.SubstateIn B",
            }, _log.Calls);
            Assert.AreSame(b, parent.CurrentState);
            Assert.AreSame(parent.Screen, a.Screen);
            Assert.AreSame(parent.Screen, b.Screen);
            Assert.AreEqual(0, _screen.DestroyCount);
        }

        [Test]
        public void SwitchState_SubstateGetsParentsScreen_NotTheInjectedOne()
        {
            var parent = Parent();
            _appFsm.SwitchState(parent);

            var otherScreen = new TestFsmScreen { Log = new CallLog() };
            _injectionBinder.Unbind<TestFsmScreen>();
            _injectionBinder.Bind<TestFsmScreen>().ToValue(otherScreen).ToInject(false);

            var a = Sub("A");
            parent.SwitchState(a);

            Assert.AreSame(_screen, a.Screen);
            CollectionAssert.IsEmpty(otherScreen.Log.Calls);
        }

        [Test]
        public void SwitchState_DuringSubstateTransition_IsQueued()
        {
            var parent = Parent();
            _appFsm.SwitchState(parent);
            _log.Calls.Clear();

            var b = Sub("B");
            var a = Sub("A");
            a.OnEnter = () => parent.SwitchState(b);
            parent.SwitchState(a);

            CollectionAssert.AreEqual(new[]
            {
                "A.Enter",
                "screen.SubstateIn A",
                "A.Exit",
                "screen.SubstateOut A",
                "A.Destroy",
                "B.Enter",
                "screen.SubstateIn B",
            }, _log.Calls);
            Assert.AreSame(b, parent.CurrentState);
        }

        [Test]
        public void Leave_ParentExitsThenSubstateExitsOnDestroy_ScreenDestroyedOnce()
        {
            var parent = Parent(Sub("A"));
            _appFsm.SwitchState(parent);
            _log.Calls.Clear();

            _appFsm.SwitchState(new TestPlainState { Name = "Next", Log = _log });

            CollectionAssert.AreEqual(new[]
            {
                "parent.Exit",
                "screen.Exit",
                "parent.UnregisterHandlers",
                "AppStateExit TestParentState",
                "A.Exit",
                "screen.SubstateOut A",
                "A.Destroy",
                "screen.Destroy",
                "Next.Enter",
                "plainScreen.Enter",
                "AppStateEnter TestPlainState",
            }, _log.Calls);
            Assert.AreEqual(1, _screen.DestroyCount);
            Assert.IsNull(parent.CurrentState);
        }

        [Test]
        public void Leave_WithoutSubstate_DestroysScreen()
        {
            var parent = Parent();
            _appFsm.SwitchState(parent);
            _log.Calls.Clear();

            _appFsm.SwitchState(new TestPlainState { Name = "Next", Log = _log });

            CollectionAssert.AreEqual(new[]
            {
                "parent.Exit",
                "screen.Exit",
                "parent.UnregisterHandlers",
                "AppStateExit TestParentState",
                "screen.Destroy",
                "Next.Enter",
                "plainScreen.Enter",
                "AppStateEnter TestPlainState",
            }, _log.Calls);
            Assert.AreEqual(1, _screen.DestroyCount);
        }

        [Test]
        public void Leave_DropsQueuedSubstateSwitches()
        {
            var a = Sub("A");
            a.HoldEnter = true;
            var parent = Parent(a);
            _appFsm.SwitchState(parent);

            parent.SwitchState(Sub("B"));
            Assert.AreEqual(TransitionType.Enter, parent.CurrentTransition);

            _appFsm.SwitchState(new TestPlainState { Name = "Next", Log = _log });

            CollectionAssert.DoesNotContain(_log.Calls, "B.Enter");
            CollectionAssert.Contains(_log.Calls, "A.Exit");
            Assert.IsNull(parent.CurrentState);
        }

        [Test]
        public void Signals_FireForParentOnly_NeverForSubstates()
        {
            var parent = Parent(Sub("A"));
            _appFsm.SwitchState(parent);
            parent.SwitchState(Sub("B"));
            _appFsm.SwitchState(new TestPlainState { Name = "Next", Log = _log });

            var signalCalls = _log.Calls.FindAll(call => call.StartsWith("AppState"));
            CollectionAssert.AreEqual(new[]
            {
                "AppStateEnter TestParentState",
                "AppStateExit TestParentState",
                "AppStateEnter TestPlainState",
            }, signalCalls);
        }

        [Test]
        public void SwitchState_NotASubState_WarnsAndGoesOnWithTheSharedScreen()
        {
            var parent = Parent();
            _appFsm.SwitchState(parent);

            var state = new TestNonSubState();
            LogAssert.Expect(LogType.Warning, new Regex("Substate not valid"));
            parent.SwitchState(state);

            Assert.AreSame(state, parent.CurrentState);
            Assert.AreSame(_screen, state.Screen);
        }
    }
}
