using System.Numerics;

public class BrakePlayerState : PlayerState
{
    protected override void OnEnter(Player player) { }

    protected override void OnExit(Player player) { }

    // ReSharper disable Unity.PerformanceAnalysis
    protected override void OnStep(Player player)
    {
        player.Decelerate();

        if (player.lateralVelocity.sqrMagnitude == 0)
        {
            player.states.Change<IdlePlayerState>();
        }
    }
}