using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    public InputActionAsset  actions;
    protected float movementDirectionUnlockTime;
    protected float? m_lasetJumpTime;
    protected InputAction m_movement;
    protected InputAction m_run;
    protected InputAction m_jump;
    protected InputAction m_look;
    protected Camera m_camera;
    
    protected const float k_jumpBuffer = 0.15f;
    protected const string k_mouseDeviceName = "Mouse";
    
    protected virtual void Awake() => CacheActions();

    // Start is called before the first frame update
    void Start()
    {
        m_camera = Camera.main;
        actions.Enable();//激活ACTIONS
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (m_jump.WasPressedThisFrame())
        {
            m_lasetJumpTime = Time.time;
        }
    }

    protected void OnEnable()
    {
        actions?.Enable();
    }

    protected void OnDisable()
    {
        actions?.Disable();
    }

    protected virtual void CacheActions()
    {
        m_movement = actions["Movement"];
        m_run = actions["Run"];
        m_jump = actions["Jump"];
        m_look = actions["Look"];
    }

    public virtual bool GetRun() => m_run.IsPressed();
    
    public virtual Vector3 GetMovementDirection()
    {
        if(Time.time < movementDirectionUnlockTime) return Vector3.zero;
        
        var value = m_movement.ReadValue<Vector2>();
        return GetAxisWithCrossDeadZone(value);
    }

    public virtual Vector3 GetAxisWithCrossDeadZone(Vector2 axis)
    {
        var deadZone = InputSystem.settings.defaultDeadzoneMin;
        axis.x = Mathf.Abs(axis.x) > deadZone ? RemapToDeadZone(axis.x , deadZone) : 0;
        axis.y = Mathf.Abs(axis.y) > deadZone ? RemapToDeadZone(axis.y , deadZone) : 0;
        return new Vector3(axis.x, 0, axis.y);
    }

    protected float RemapToDeadZone(float value, float deadZone) => (value - deadZone) / (1-deadZone);

    public virtual Vector3 GetMovementCameraDirection()
    {
        var direction = GetMovementDirection();

        if (direction.sqrMagnitude > 0)
        {
            var rotation = Quaternion.AngleAxis(m_camera.transform.eulerAngles.y, Vector3.up);
            direction = rotation * direction;
            direction = direction.normalized;
        }
        return direction;
    }
    
    public virtual bool GetJumpDown()
    {
        if (m_lasetJumpTime != null && Time.time - m_lasetJumpTime >= k_jumpBuffer)
        {
            m_lasetJumpTime = null;
            return true;
        }
        return false;
    }

    public virtual bool GetJumpUp() => m_jump.WasReleasedThisFrame();
    
    public virtual bool IsLookingWithMouse()
    { 
       if(m_look.activeControl == null) return false;

       return m_look.activeControl.device.name.Equals(k_mouseDeviceName);
    }

    public virtual Vector3 GetLookDirection()
    {
        var value = m_look.ReadValue<Vector2>();
        if (IsLookingWithMouse())
        {
            return new Vector3(value.x, 0, value.y);
        }
        
        return GetAxisWithCrossDeadZone(value);
    }
}
