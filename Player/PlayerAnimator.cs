using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [System.Serializable] //可序列化
    public class ForcedTranstion
    {
        public int fromStateId;
        public int animatorLayer;
        public string toAnimationState;
    }

    protected Dictionary<int, ForcedTranstion> m_forcedTranstions;
    protected Player m_player;

    public Animator animator;

    [Header("Parameters Names")]
    public string stateName = "State";
    public string lastStateName = "Last State";
    public string lateralSpeedName = "Lateral Speed";
    public string verticalSpeedName = "Vertical Speed";
    public string lateralAnimationSpeedName = "Lateral Animation Speed";
    public string healthName = "Health";
    public string jumpCounterName = "Jump Counter";
    public string isGroundedName = "Is Grounded";
    public string isHoldingName = "Is Holding";
    public string onStateChangedName = "On State Changed";

    [Header("Settings")] 
    public float minLaterAnimationSpeed = 0.5f;
    public List<ForcedTranstion> forcedTranstions;

    protected int m_stateHash;
    protected int m_lastStateHash;
    protected int m_lateralSpeedHash;
    protected int m_verticalSpeedHash;
    protected int m_lateralAnimationSpeedHash;
    protected int m_healthHash;
    protected int m_jumpCounterHash;
    protected int m_isGroundedHash;
    protected int m_isHoldingHash;
    protected int m_onStateChangedHash;

    protected void Start()
    {
        InitializePlayer();
        InitializeForcedTranstion();
        InitializeParameterHash();
        InitializeAnimatorTriggers();
    }

    protected virtual void InitializePlayer()
    {
        m_player = GetComponent<Player>();
        m_player.states.events.onChange.AddListener(HandleForcedTranstion);
    }

    protected virtual void InitializeForcedTranstion()
    {
        m_forcedTranstions = new Dictionary<int, ForcedTranstion>();

        foreach (var transtion in forcedTranstions)
        {
            if (!m_forcedTranstions.ContainsKey(transtion.fromStateId))
            {
                m_forcedTranstions.Add(transtion.fromStateId, transtion);
            }
        }
    }

    protected virtual void InitializeParameterHash()
    {
        // 对应的将animator中变量的名称都转换成哈希值，方便我们后续使用
        m_stateHash = Animator.StringToHash(stateName);
        m_lastStateHash = Animator.StringToHash(lastStateName);
        m_lateralSpeedHash = Animator.StringToHash(lateralSpeedName);
        m_verticalSpeedHash = Animator.StringToHash(verticalSpeedName);
        m_lateralAnimationSpeedHash = Animator.StringToHash(lateralAnimationSpeedName);
        m_healthHash = Animator.StringToHash(healthName);
        m_jumpCounterHash = Animator.StringToHash(jumpCounterName);
        m_isGroundedHash = Animator.StringToHash(isGroundedName);
        m_isHoldingHash = Animator.StringToHash(isHoldingName);
        m_onStateChangedHash = Animator.StringToHash(onStateChangedName);
    }

    protected virtual void InitializeAnimatorTriggers()
    {
        m_player.states.events.onChange.AddListener(() => animator.SetTrigger(m_onStateChangedHash));
    }

    protected virtual void HandleForcedTranstion()
    {
        var lastStateIndex = m_player.states.lastIndex;

        if (m_forcedTranstions.ContainsKey(lastStateIndex))
        {
            var layer = m_forcedTranstions[lastStateIndex].animatorLayer;
            animator.Play(m_forcedTranstions[lastStateIndex].toAnimationState, layer);
        }
    }

    protected virtual void HandleAnimatorParameter()
    {
        var lateralSpeed = m_player.lateralVelocity.magnitude;
        var verticalSpeed = m_player.verticalVelocity.y;
        var lateralAnimationSpeed = Mathf.Max(minLaterAnimationSpeed, lateralSpeed / m_player.stats.current.topSpeed);
        
        animator.SetInteger(m_stateHash, m_player.states.index);
        animator.SetInteger(m_lastStateHash, m_player.states.lastIndex);
        animator.SetFloat(m_lateralSpeedHash, lateralSpeed);
        animator.SetFloat(m_verticalSpeedHash, verticalSpeed);
        animator.SetFloat(m_lateralAnimationSpeedHash, lateralAnimationSpeed);
        animator.SetInteger(m_jumpCounterHash, m_player.jumpCounter);
        animator.SetBool(m_isGroundedHash, m_player.isGrounded);
        animator.SetBool(m_isHoldingHash, m_player.holding);
    }

    protected virtual void LateUpdate() => HandleAnimatorParameter();
}