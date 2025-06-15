using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Entity<Player>
{
    public PlayerEvents playerEvent;
    public PlayerInputManager inputs { get; protected set; }
    public PlayerStatsManager stats { get; protected set; }
    public int jumpCounter { get; protected set; }
    public bool holding { get; protected set; }

    public virtual void FaceDirectionSmooth(Vector3 direction) => FaceDirection(direction,stats.current.rotationSpeed);
    public virtual void Decelerate() => Decelerate(stats.current.deceleration);
    protected virtual void InitializeInputs() => inputs = GetComponent<PlayerInputManager>();
    protected virtual void InitializeStates() => stats = GetComponent<PlayerStatsManager>();

    protected override void Awake()
    {
        base.Awake();
        InitializeInputs();
        InitializeStates();
    }

    public virtual void Accelerate(Vector3 direction)
    {
        var turningDrag = (isGrounded && inputs.GetRun())
            ? stats.current.runningTurningDrag
            : stats.current.turningDrag;
        var acceleration = (isGrounded && inputs.GetRun())
            ? stats.current.runningAcceleration
            : stats.current.acceleration;
        var topSpeed = inputs.GetRun()
            ? stats.current.runningTopSpeed
            : stats.current.topSpeed;
        var finalAcceleration = isGrounded ? acceleration : stats.current.airAcceleration;
        
        Accelerate(direction, turningDrag, finalAcceleration, topSpeed);
    }

    public virtual void Friction()
    {
        Decelerate(stats.current.friction);
    }

    public virtual void Gravity()
    {
        if (!isGrounded && verticalVelocity.y > -stats.current.gravityTopSpeed)
        {
          var speed = verticalVelocity.y;
          var force = verticalVelocity.y > 0 ? stats.current.gravity : stats.current.fallGravity;
          speed -= force * gravityMuliplier * Time.deltaTime;
          speed = Mathf.Max(speed, -stats.current.gravityTopSpeed);
          verticalVelocity = new Vector3(0,speed,0);
        }
    }

    public virtual void Fall()
    {
        if (!isGrounded)
        {
            states.Change<FallPlayerState>();
        }
    }

    public virtual void Jump()
    {
        var canMultiJump = (jumpCounter > 0) && (jumpCounter < stats.current.multiJumps);
        var canCoyoteJump =(jumpCounter ==0 && (Time.time < lastGroundTime + stats.current.coyoteJumpThreshold));

        if (isGrounded || canMultiJump || canCoyoteJump)
        {
            if (inputs.GetJumpDown())
            {
                Jump(stats.current.maxJumpHeight);
            }
        }

        if (inputs.GetJumpUp() && (jumpCounter > 0) && verticalVelocity.y < stats.current.minJumpHeight)
        {
            verticalVelocity = Vector3.up * stats.current.minJumpHeight;
        }
    }

    public virtual void Jump(float height)
    {
        jumpCounter++;
        verticalVelocity = Vector3.up * height;
        states.Change<FallPlayerState>();
        playerEvent.OnJump?.Invoke();
    }
}