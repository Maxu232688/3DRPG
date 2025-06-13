using System;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
}

public abstract class Entity<T> : Entity where T : Entity<T>
{
    public EntityStateManager<T> states { get; protected set; }

    protected virtual void HandleState() => states.Step();

    protected virtual void InitializeStateManager() => states = GetComponent<EntityStateManager<T>>();

    public Vector3 Velocity { get; protected set; }
    public float turningDragMultiplier { get; protected set; } = 1f;
    public float topSpeedMultiplier { get; protected set; } = 1f;
    public float acclerationMultiplier { get; protected set; } = 1f;
    public float decelerationMultipiler { get; protected set; } = 1f;
    public float gravityMuliplier { get; protected set; } = 1f;
    public bool isGrounded { get; protected set; } = true;
    public CharacterController controller { get; protected set; }
    
    public Vector3 lateralVelocity
    {
        get { return new Vector3(Velocity.x, 0, Velocity.z); }
        set { Velocity = new Vector3(value.x, Velocity.y, value.z); }
    }
    public Vector3 verticalVelocity
    {
        get { return new Vector3(0, Velocity.y, 0); }
        set { Velocity = new Vector3(Velocity.x, value.y, Velocity.z); }
    }

    protected virtual void InitializeController()
    {
        controller = GetComponent<CharacterController>();

        if (!controller)
        {
            controller = gameObject.AddComponent<CharacterController>();
        }

        controller.skinWidth = 0.005f;
        controller.minMoveDistance = 0;
    } 
    
    protected virtual void Awake()
    {
        InitializeStateManager();
        InitializeController();
    }

    protected void Update()
    {
        HandleState();
        HandleController();
    }

    protected virtual void HandleController()
    {
        if (controller.enabled)
        {
            controller.Move(Velocity * Time.deltaTime);
            return;
        }
        
        transform.position += Velocity * Time.deltaTime;
    }
    
    public virtual void Accelerate(Vector3 direction, float turningDrag, float finalAcceleration, float topSpeed)
    {
        if (direction.sqrMagnitude > 0)
        {
            var speed = Vector3.Dot(direction, lateralVelocity);
            var newVelocity = direction * speed;
            var turningVelocity = lateralVelocity - newVelocity;
            var turningDelta = turningDrag * turningDragMultiplier * Time.deltaTime;
            var targetTopSpeed = topSpeed * topSpeedMultiplier;

            if (lateralVelocity.magnitude < targetTopSpeed || speed < 0)
            {
                speed += finalAcceleration * acclerationMultiplier * Time.deltaTime;
                speed = Mathf.Clamp(speed, -targetTopSpeed, targetTopSpeed);
            }
            
            newVelocity = direction * speed;
            turningVelocity = Vector3.MoveTowards(turningVelocity, Vector3.zero, turningDelta);
            lateralVelocity = newVelocity + turningVelocity ;
        }
    }

    public virtual void FaceDirection(Vector3 direction, float degreesPerSpeed)
    {
        if (direction != Vector3.zero)
        {
            var rotation = transform.rotation;
            var rotationDelta = degreesPerSpeed * Time.deltaTime;
            var target = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(rotation, target, rotationDelta);
        }
    }

    public virtual void Decelerate(float deceleration)
    {
        var delta = deceleration * decelerationMultipiler * Time.deltaTime;
        lateralVelocity = Vector3.MoveTowards(lateralVelocity, Vector3.zero, delta);
    }
}