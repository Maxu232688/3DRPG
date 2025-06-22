using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class PlayerCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    public Player player;
    public float maxDistance = 15f;
    public float initAngle = 20f;
    public float heightOffset = 1f;
    
    [Header("Following Settings")]
    public float verticalUpDeadZone = 0.15f;
    public float verticalDownDeadZone = 0.15f;
    public float verticalAirUpDeadZone = 4f;
    public float verticalAirDownDeadZone = 0f;
    public float maxVerticalSpeed = 10f;
    public float maxVerAirSpeed = 100f;
    
    [Header("Orbit Settings")]
    public bool orbitEnabled = true;
    public bool orbitEnabledVelocity = true;
    public float orbitEnabledVelocityMultiplier = 5f;
    
    [Range(0f, 90f)]
    public float verticalMaxRotation = 80f;
    [Range(0f, 90f)]
    public float verticalMinRotation = -20f;

    protected CinemachineVirtualCamera m_camera;
    protected Cinemachine3rdPersonFollow m_cameraBody;
    protected CinemachineBrain m_brain;
    protected Transform m_target;
    protected float m_cameraDistance;
    protected Vector3 m_cameraTargetPosition;
    protected float m_cameraTargetYaw;//偏转角
    protected float m_cameraTargetPitch;//摇动
    
    
    protected string k_targetName = "Player Follower Camera";
    protected virtual void InitializeComponents()
    {
        if (!player) player = FindObjectOfType<Player>();
        
        m_camera = GetComponent<CinemachineVirtualCamera>();
        m_cameraBody = m_camera.AddCinemachineComponent<Cinemachine3rdPersonFollow>();
        m_brain = Camera.main.GetComponent<CinemachineBrain>();
    }

    protected virtual void InitializeFollower()
    {
        m_target = new GameObject(k_targetName).transform;
        m_target.position = player.transform.position;
    }

    protected virtual void InitializeCamera()
    {
        m_camera.Follow = m_target.transform;
        m_camera.LookAt = player.transform;
        Reset();
    }

    public virtual void Reset()
    {
        m_cameraDistance = maxDistance;
        m_cameraTargetPitch = initAngle;
        m_cameraTargetYaw = player.transform.eulerAngles.y;
        m_cameraTargetPosition = player.unsizePosition + Vector3.up * heightOffset;
        MoveTarget();
        m_brain.ManualUpdate();
    }

    protected virtual void MoveTarget()
    {
        m_target.position = m_cameraTargetPosition;
        m_target.rotation = Quaternion.Euler( m_cameraTargetPitch, m_cameraTargetYaw, 0f);
        m_cameraBody.CameraDistance = m_cameraDistance;
    }

    protected virtual void HandleOrbit()
    {
        if (orbitEnabled)
        {
            var direction = player.inputs.GetLookDirection();

            if (direction.magnitude > 0)
            {
                var usingMouse = player.inputs.IsLookingWithMouse();
                float deltaTimeMultiplier = usingMouse ? Time.timeScale : Time.deltaTime;
                
                m_cameraTargetYaw += direction.x * deltaTimeMultiplier;
                m_cameraTargetPitch -= direction.z * deltaTimeMultiplier;
                m_cameraTargetPitch = ClampAngle(m_cameraTargetPitch, verticalMinRotation, verticalMaxRotation);
            }
        }
    }

    protected virtual void HandleVelocityOrbit()
    {
        if (orbitEnabledVelocity && player.isGrounded)
        {
            var localVelocity = m_target.InverseTransformVector(player.Velocity);
            m_cameraTargetYaw += localVelocity.x * orbitEnabledVelocityMultiplier * Time.deltaTime;
        }
    }

    protected virtual bool VerticalFollowingSate()
    {
        return false;
    }

    protected virtual void HandleOffset()
    {
        var target = player.unsizePosition + Vector3.up * heightOffset;
        var previousPosition = m_cameraTargetPosition;
        var targetHeight = previousPosition.y;

        if (player.isGrounded || VerticalFollowingSate())
        {
            if (target.y > previousPosition.y + verticalUpDeadZone)
            {
                var offset = target.y - previousPosition.y - verticalUpDeadZone;
                targetHeight += Mathf.Min(offset, maxVerticalSpeed * Time.deltaTime);
            }
            else if (target.y < previousPosition.y - verticalDownDeadZone)
            {
                var offset = target.y - previousPosition.y + verticalDownDeadZone;
                targetHeight += Mathf.Max(offset, -maxVerticalSpeed * Time.deltaTime);
            }
        }
        else if (target.y > previousPosition.y + verticalAirUpDeadZone)
        {
            var offset = target.y - previousPosition.y - verticalAirUpDeadZone;
            targetHeight += Mathf.Min(offset, maxVerAirSpeed * Time.deltaTime);
        }
        else if (target.y < previousPosition.y - verticalAirDownDeadZone)
        {
            var offset = target.y - previousPosition.y + verticalAirUpDeadZone;
            targetHeight += Mathf.Min(offset, -maxVerAirSpeed * Time.deltaTime);
        }
        
        m_cameraTargetPosition  = new Vector3(target.x, targetHeight, target.z);
    }

    protected virtual float ClampAngle(float angle, float min, float max)
    {
        if(angle < -360) angle += 360;
        if(angle > 360) angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
    
    protected virtual void Start()
    {
        InitializeComponents();
        InitializeFollower();
        InitializeCamera();
    }

    protected void LateUpdate()
    {
        HandleOrbit();
        HandleVelocityOrbit();
        HandleOffset();
        MoveTarget();
    }
}