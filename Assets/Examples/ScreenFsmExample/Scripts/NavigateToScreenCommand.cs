using Framewerk.AppStateMachine;
using strange.extensions.command.impl;
using UnityEngine;

namespace Framewerk.Examples.ScreenFsmExample
{
    public class NavigateToScreenCommand : Command
    {
        [Inject] public ScreenType ScreenType { get; set; }
        [Inject] public IAppFsm Fsm { get; set; }

        public override void Execute()
        {
            Debug.Log($"[ScreenFsmExample] Navigating to: {ScreenType}");

            IAppState state = ScreenType switch
            {
                ScreenType.About => new AboutState(),
                ScreenType.Game => new GameState(),
                ScreenType.Settings => new SettingsState(),
                ScreenType.Leaderboards => new LeaderboardsState(),
                _ => null
            };

            if (state != null)
                Fsm.SwitchState(state);
        }
    }
}
