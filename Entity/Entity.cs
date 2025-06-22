using System;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public EntityEvents entityEvents;
    public float turningDragMultiplier { get; protected set; } = 1f;
    public float topSpeedMultiplier { get; protected set; } = 1f;
    public float acclerationMultiplier { get; protected set; } = 1f;
    public float decelerationMultipiler { get; protected set; } = 1f;
    public float gravityMuliplier { get; protected set; } = 1f;
    public float lastGroundTime { get; protected set; }
    public bool isGrounded { get; protected set; } = true;
    public Vector3 Velocity { get; protected set; }
    public CharacterController controller { get; protected set; }
    public readonly float m_groundOffset = 0.1f;
    public float originalHeight { get; protected set; }
    public Vector3 unsizePosition => position - transform.up * height * 0.5f + transform.up * originalHeight * 0.5f;
    
    public RaycastHit groundHit;

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

    public float height => controller.height;
    public float radius => controller.radius;
    public Vector3 center => controller.center;
    public Vector3 position => transform.position + center;

    public virtual bool SphereCast(Vector3 direction, float distance, out RaycastHit hit,
        int layer = Physics.DefaultRaycastLayers,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
    {
        var castDistance = Mathf.Abs(distance - radius);
        return Physics.SphereCast(position, radius, direction, out hit, castDistance, layer, queryTriggerInteraction);
    }

    public Vector3 stepPosition => position - transform.up * (height * 0.5f - controller.stepOffset);
    public virtual bool IsPointUnderStep(Vector3 point)=> stepPosition.y > point.y;
}

public abstract class Entity<T> : Entity where T : Entity<T>
{
    public EntityStateManager<T> states { get; protected set; }

    protected virtual void HandleState() => states.Step();

    protected virtual void InitializeStateManager() => states = GetComponent<EntityStateManager<T>>();

    protected virtual void InitializeController()
    {
        controller = GetComponent<CharacterController>();

        if (!controller)
        {
            controller = gameObject.AddComponent<CharacterController>();
        }

        controller.skinWidth = 0.005f;
        controller.minMoveDistance = 0;
        originalHeight = controller.height;
    }

    protected virtual void Awake()
    {
        InitializeStateManager();
        InitializeController();
    }

    protected virtual void Update()
    {
        if (controller.enabled)
        {
            HandleState();
            HandleController();
            HandleGround();
        }
    }

    protected virtual void HandleGround()
    {
        var distance = (height * 0.5f) + m_groundOffset;

        if (SphereCast(Vector3.down, distance, out var hit) && verticalVelocity.y <= 0)
        {
            if (!isGrounded)
            {
                if (EvaluateLanding(hit))
                {
                    EnterGround(hit);
                }
                else
                {
                    HandleHighLedge(hit);
                }
            }
        }
        else
        {
            ExitGround();
        }
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
            lateralVelocity = newVelocity + turningVelocity;
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

    protected virtual void EnterGround(RaycastHit hit)
    {
        if (!isGrounded)
        {
            groundHit = hit;
            isGrounded = true;
            entityEvents.OnGroundEnter?.Invoke();
        }
    }
    
    protected virtual void ExitGround()
    {
        if (isGrounded)
        {
            isGrounded = false;
            transform.parent = null;
            lastGroundTime = Time.time;
            verticalVelocity = Vector3.Max(verticalVelocity, Vector3.zero);
            entityEvents.OnGroundExit?.Invoke();
        }
    }

    protected virtual bool EvaluateLanding(RaycastHit hit)
    {
       return IsPointUnderStep(hit.point) && Vector3.Angle(hit.normal, Vector3.up) < controller.slopeLimit;
    }
    
    protected virtual void HandleHighLedge(RaycastHit hit)
    {
        // 空着先不写
    }
}