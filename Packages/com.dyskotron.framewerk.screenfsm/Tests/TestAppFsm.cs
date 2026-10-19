using System;
using NUnit.Framework;
using strange.extensions.injector.api;
using strange.extensions.injector.impl;
using strange.framework.api;

namespace Framewerk.AppStateMachine.Tests
{
    /// <summary>
    /// Pins AppFsm's behaviour: enter / exit order, queueing, destroy after exit, the app-wide signals.
    /// </summary>
    [TestFixture]
    public class TestAppFsm
    {
        private IInjectionBinder _injectionBinder;
        private CallLog _log;
        private IAppFsm _fsm;

        [SetUp]
        public void SetUp()
        {
            _log = new CallLog();
            _injectionBinder = new InjectionBinder();
            _injectionBinder.Bind<IInjectionBinder>().Bind<IInstanceProvider>().ToValue(_injectionBinder);
            _injectionBinder.Bind<IAppFsm>().To<AppFsm>().ToSingleton();
            _injectionBinder.Bind<AppStateEnterSignal>().ToSingleton();
            _injectionBinder.Bind<AppStateExitSignal>().ToSingleton();
            _injectionBinder.Bind<TestPlainScreen>().ToValue(new TestPlainScreen { Log = _log }).ToInject(false);

            _injectionBinder.GetInstance<AppStateEnterSignal>().AddListener(type => _log.Add($"AppStateEnter {type.Name}"));
            _injectionBinder.GetInstance<AppStateExitSignal>().AddListener(type => _log.Add($"AppStateExit {type.Name}"));

            _fsm = _injectionBinder.GetInstance<IAppFsm>();
        }

        private TestPlainState State(string name, Action onEnter = null)
        {
            return new TestPlainState { Name = name, Log = _log, OnEnter = onEnter };
        }

        [Test]
        public void SwitchState_FromEmpty_EntersStateAndDispatchesEnterSignal()
        {
            var state = State("A");

            _fsm.SwitchState(state);

            CollectionAssert.AreEqual(new[]
            {
                "A.Enter",
                "plainScreen.Enter",
                "AppStateEnter TestPlainState",
            }, _log.Calls);
            Assert.AreSame(state, _fsm.CurrentState);
            Assert.AreEqual(TransitionType.None, _fsm.CurrentTransition);
        }

        [Test]
        public void SwitchState_ExitsAndDestroysCurrentThenEntersNext()
        {
            _fsm.SwitchState(State("A"));
            _log.Calls.Clear();

            var b = State("B");
            _fsm.SwitchState(b);

            CollectionAssert.AreEqual(new[]
            {
                "A.Exit",
                "plainScreen.Exit",
                "AppStateExit TestPlainState",
                "A.Destroy",
                "plainScreen.Destroy",
                "B.Enter",
                "plainScreen.Enter",
                "AppStateEnter TestPlainState",
            }, _log.Calls);
            Assert.AreSame(b, _fsm.CurrentState);
        }

        [Test]
        public void SwitchState_DuringEnter_IsQueuedUntilEnterFinished()
        {
            var b = State("B");
            _fsm.SwitchState(State("A", () => _fsm.SwitchState(b)));

            CollectionAssert.AreEqual(new[]
            {
                "A.Enter",
                "plainScreen.Enter",
                "AppStateEnter TestPlainState",
                "A.Exit",
                "plainScreen.Exit",
                "AppStateExit TestPlainState",
                "A.Destroy",
                "plainScreen.Destroy",
                "B.Enter",
                "plainScreen.Enter",
                "AppStateEnter TestPlainState",
            }, _log.Calls);
            Assert.AreSame(b, _fsm.CurrentState);
        }

        [Test]
        public void SwitchState_InjectsTheState()
        {
            var state = State("A");

            _fsm.SwitchState(state);

            Assert.AreSame(_injectionBinder.GetInstance<TestPlainScreen>(), state.Screen);
        }
    }
}
