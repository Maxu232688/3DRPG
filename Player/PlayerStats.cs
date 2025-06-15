using UnityEngine;

public class PlayerStats : EntityStats<PlayerStats>
{
    [Header("General Stats")]
    public float rotationSpeed = 940f;
    public float friction = 16f;
    public float gravityTopSpeed = 50f;
    public float gravity = 50f;
    public float fallGravity = 65f;
    
    [Header("Motion Stats")] 
    public float brakeThreshold = -0.8f;
    public float turningDrag = 28f;
    public float acceleration = 13f;
    public float topSpeed = 6f;
    public float airAcceleration = 32f;
    public float deceleration = 28f;

    [Header("Running Stats")] 
    public float runningAcceleration = 16f;
    public float runningTopSpeed = 7.5f;
    public float runningTurningDrag = 14f;
    
    [Header("Jump Stats")]
    public int multiJumps = 1;
    public float coyoteJumpThreshold = 0.15f;//土狼时间
    public float maxJumpHeight = 17f;
    public float minJumpHeight = 10f;
}