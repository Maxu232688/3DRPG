using System;
using UnityEngine.Events;

[Serializable]
public class EntityEvents
{
    public UnityEvent OnGroundEnter;
    public UnityEvent OnGroundExit;
    public UnityEvent OnRailEnter;
    public UnityEvent OnRailExit;
}