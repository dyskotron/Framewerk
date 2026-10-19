using Framewerk.AppStateMachine;
using UnityEngine;

namespace Framewerk.Examples.ScreenFsmExample
{
    public class GameScreen : FsmStateScreen
    {
        private GameSubstateView _substateView;
        private string _substateName;

        protected override async void Enter()
        {
            await InstantiateViewAsync<GameContentView>();
            _substateView = await InstantiateViewAsync<GameSubstateView>();
            _substateView.SetSubstate(_substateName);
        }

        public override void SubstateIn(IAppState state)
        {
            _substateName = state.GetType().Name;
            Debug.Log($"[ScreenFsmExample] Substate in: {_substateName}");

            if (_substateView != null)
                _substateView.SetSubstate(_substateName);
        }

        public override void SubstateOut(IAppState state)
        {
            Debug.Log($"[ScreenFsmExample] Substate out: {state.GetType().Name}");
        }

        public override void Destroy()
        {
            base.Destroy();
            _substateView = null;
        }
    }
}
