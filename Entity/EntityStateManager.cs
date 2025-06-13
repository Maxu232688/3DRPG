using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityStateManager : MonoBehaviour
{
    public EntityStateManagerEvents events;
}

public abstract class EntityStateManager<T> : EntityStateManager where T : Entity<T>
{
    protected List<EntityState<T>> m_list = new List<EntityState<T>>();
    protected Dictionary<Type,EntityState<T>> m_states = new Dictionary<Type,EntityState<T>>();
    protected abstract List<EntityState<T>> GetStateList();
    public EntityState<T> current { get; protected set; }
    public EntityState<T> last { get; protected set; }
    public int lastIndex => m_list.IndexOf(last);
    public int index => m_list.IndexOf(current);
    public T entity { get; protected set; }
    protected virtual void InitializeEntity() => entity = GetComponent<T>();

    public virtual void InitializeStates()
    {
        m_list = GetStateList();

        foreach (var state in m_list)
        {
            var type = state.GetType();

            if (!m_states.ContainsKey(type))
            {
                m_states.Add(type,state);
            }
        }

        if (m_list.Count > 0)
        {
            current = m_list[0];
        }
    }

    protected virtual void Start()
    {
        InitializeEntity();
        InitializeStates();
    }
    public virtual void Step()
    {
        if (current != null && Time.timeScale > 0)
        {
            current.Step(entity);
        }
    }

    public virtual void Change<TState>() where TState : EntityState<T>
    {
        var type = typeof(TState);
        if (m_states.ContainsKey(type))
        {
            Change(m_states[type]);
        }
    }

    public virtual void Change(EntityState<T> to)
    {
        if (to != null && Time.timeScale > 0)
        {
            if (current != null)
            {
                current.Exit(entity);
                last = current;
            }
            
            current = to;
            current.Enter(entity);
        }
    }
}