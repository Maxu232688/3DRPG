using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Collider))]
public class Collectable : MonoBehaviour
{
    [Header("General Settings")]
    public GameObject display;
    public bool collectOnContact;
    public int times;
    public ParticleSystem particle;
    public float ghostingDuration = 0.5f;
    public AudioClip clip;

    [Header("Visibility Settings")] 
    public bool hidden;
    public float quickShowHeight = 2f;
    public float quickShowDuration = 0.25f;
    public float hideDuration = 0.5f;

    [Header("Life Settings")] 
    public bool hasLifeTime;
    public float LifeTimeDuration = 5f;

    [Header("Physics Settings")] 
    public Vector3 initVelocity = new Vector3(0, 12, 0);
    public bool useGravity;
    public float gravity = 15f;
    public float collisionRadius = 0.5f;
    public float bounciness = 0.9f;
    public float maxBounceYVelocity = 10f;
    public float minForceToStopPhysics = 10f;
    public AudioClip collisionClip;

    public bool randomizeDirection = true;

    [Space(15)] 
    public PlayerEvent onCollect;
    
    protected AudioSource m_audio;
    protected Collider m_collider;
    protected Vector3 m_velocity;
    protected bool m_vanished;
    protected bool m_ghosting = true;
    protected float m_passedGhostingTime;
    protected float m_passedLifeTime;

    protected const int k_verticalMinRotation = 0;
    protected const int k_verticalMaxRotation = 30;
    protected const int k_horizontalMinRotation = 0;
    protected const int k_horizontalMazRotation = 360;

    protected virtual void InitializeAudio()
    {
        if (!TryGetComponent(out m_audio))
        {
            m_audio = GetComponent<AudioSource>();
        }
    }

    protected virtual void InitializeCollider()
    {
        m_collider = GetComponent<Collider>();
        m_collider.isTrigger = true;
    }

    protected virtual void InitializeTransform()
    {
        transform.parent = null;
        transform.rotation = Quaternion.identity;
    }

    protected virtual void InitializeDisplay()
    {
        display.SetActive(!hidden);
    }

    protected virtual void InitializeVelocity()
    {
        var direction = initVelocity.normalized;
        var force = initVelocity.magnitude;

        if (randomizeDirection)
        {
            var randomZ = UnityEngine.Random.Range(k_verticalMinRotation, k_verticalMaxRotation);
            var randomY = UnityEngine.Random.Range(k_horizontalMinRotation, k_horizontalMazRotation);
            direction = Quaternion.Euler(0, randomY, 0) * direction;
            direction = Quaternion.Euler(0, 0, randomZ) * direction;
        }

        m_velocity = direction * force;
    }

    protected virtual void HandleGhosting()
    {
        if (m_ghosting)
        {
            m_passedGhostingTime += Time.deltaTime;

            if (m_passedGhostingTime >= ghostingDuration)
            {
                m_passedGhostingTime = 0;
                m_ghosting = false;
            }
        }
    }

    protected virtual void HandleLeftTime()
    {
        if (hasLifeTime)
        {
            m_passedLifeTime += Time.deltaTime;

            if (m_passedLifeTime >= LifeTimeDuration)
            {
                Vanish();
                m_passedLifeTime = 0;
            }
        }
    }

    public virtual void Vanish()
    {
        if (!m_vanished)
        {
            m_vanished = true;
            m_passedLifeTime = 0;
            display.SetActive(false);
            m_collider.enabled = false;
        }
    }

    protected virtual void HandleMovement()
    {
        m_velocity.y -= gravity * Time.deltaTime;
    }
    
    protected virtual void HandleSweep()
    {
        var direction = m_velocity.normalized;
        var magnitude = m_velocity.magnitude;
        var distance = magnitude * Time.deltaTime;

        if (Physics.SphereCast(transform.position, collisionRadius , direction, out var hit, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            if (!hit.collider.CompareTag(GameTag.PlayerTag))
            {
                var bounceDirection = Vector3.Reflect(direction, hit.normal);
                m_velocity = bounceDirection * magnitude * bounciness;
                m_velocity.y = Mathf.Min(m_velocity.y, maxBounceYVelocity);
                m_audio.Stop();
                m_audio.PlayOneShot(collisionClip);

                if (m_velocity.y < minForceToStopPhysics)
                {
                   useGravity = false; 
                }
            }
        }
        
        transform.position += m_velocity * Time.deltaTime;
    }

    public virtual void Collect(Player player)
    {
        if (!m_vanished && !m_ghosting)
        {
            if (!hidden)
            {
                Vanish();

                if (particle != null)
                {
                    particle.Play();
                }
            }
            else
            {
                StartCoroutine(QuickShowRoutine());
            }
            
            StartCoroutine(CollectRoutine(player));
        }
    }

    protected virtual IEnumerator CollectRoutine(Player player)
    {
        for (int i = 0; i < times; i++)
        {
            m_audio.Stop();
            m_audio.PlayOneShot(clip);
            onCollect.Invoke(player);
            yield return new WaitForSeconds(0.1f);
        }
    }

    protected virtual IEnumerator QuickShowRoutine()
    {
        var leftTime = 0f;
        var initialPosition = transform.position;
        var targetPosition = initialPosition + Vector3.up * quickShowHeight;
        
        display.SetActive(true);
        m_collider.enabled = false;

        while (leftTime < quickShowDuration)
        {
            var time = leftTime / quickShowDuration;
            transform.position = Vector3.Lerp(initialPosition, targetPosition, time);
            leftTime += Time.deltaTime;
            yield return null;
        }
        
        transform.position = targetPosition;
        yield return new WaitForSeconds(hideDuration);
        transform.position = initialPosition;
        Vanish();
    }
    protected void Awake()
    {
        InitializeAudio();
        InitializeCollider();
        InitializeTransform();
        InitializeDisplay();
        InitializeVelocity();
    }

    protected virtual void Update()
    {
        if (!m_vanished)
        {
            HandleGhosting();
            HandleLeftTime();

            if (useGravity)
            {
                HandleMovement();
                HandleSweep();
            }
        }
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        if (collectOnContact && other.CompareTag(GameTag.PlayerTag))
        {
            if (other.TryGetComponent<Player>(out var player))
            {
                Collect(player);
            }
        }
    }

    protected virtual void OnDrawGizmos()
    {
        if (useGravity)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, collisionRadius);
        }
    }
}