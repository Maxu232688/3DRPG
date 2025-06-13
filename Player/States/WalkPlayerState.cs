using UnityEngine;

public class WalkPlayerState : PlayerState
{
    protected override void OnEnter(Player player) { }

    protected override void OnExit(Player player) { }

    // ReSharper disable Unity.PerformanceAnalysis
    protected override void OnStep(Player player)
    {
        player.Gravity();
        player.Fall();
        
        var inputDirection = player.inputs.GetMovementCameraDirection();

        if (inputDirection.sqrMagnitude > 0)
        {
            var dot = Vector3.Dot(inputDirection, player.lateralVelocity);

            if (dot >= player.stats.current.brakeThreshold)
            {
                 player.Accelerate(inputDirection);
                 player.FaceDirectionSmooth(player.lateralVelocity);
            }
            else
            {
                player.states.Change<BrakePlayerState>();
            }
           
        }
        else
        {
            player.Friction();

            if (player.lateralVelocity.sqrMagnitude <= 0)
            {
                player.states.Change<IdlePlayerState>();
            }
        }
    }
}