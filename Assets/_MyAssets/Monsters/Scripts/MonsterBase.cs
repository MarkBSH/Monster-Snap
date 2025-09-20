using System.IO;
using UnityEngine;
using UnityEngine.AI;

public class MonsterBase : MonoBehaviour
{
    #region Unity Methods

    protected virtual void Awake()
    {
        m_AudioSource = GetComponent<AudioSource>();
        m_Animator = GetComponent<Animator>();
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
        m_Players = GameObject.FindGameObjectsWithTag("Player");
    }

    protected virtual void Start()
    {
        StartRoaming();
    }

    protected virtual void Update()
    {
        CloseToDestination();
    }

    #endregion

    #region Components

    [Header("Components"), Space(5)]
    protected AudioSource m_AudioSource;
    protected Animator m_Animator;
    protected NavMeshAgent m_NavMeshAgent;
    protected GameObject[] m_Players;

    #endregion

    #region Monster Properties

    [Header("Monster Properties"), Space(5)]
    public string m_MonsterName;
    public float m_BaseScore;
    public float m_ActionMultiplier = 1f;
    public int m_AttackPower;

    public enum MonsterState
    {
        Roaming,
        Stalking,
        Fleeing
    }
    public MonsterState m_CurrentState = MonsterState.Roaming;

    #endregion

    #region Monster Actions

    [Header("Monster Actions"), Space(5)]
    Vector3 m_NavMeshDestination;

    public virtual void StartRoaming()
    {
        m_CurrentState = MonsterState.Roaming;
    }

    public virtual void StartStalking()
    {
        m_CurrentState = MonsterState.Stalking;
    }

    public virtual void StartFleeing()
    {
        m_CurrentState = MonsterState.Fleeing;
    }

    public virtual void NavMeshDestination(Vector3 destination, System.Action onFailCallback = null)
    {
        NavMeshPath navMeshPath = new();
        m_NavMeshAgent.CalculatePath(destination, navMeshPath);
        if (navMeshPath.status == NavMeshPathStatus.PathComplete)
        {
            m_NavMeshDestination = destination;
            m_NavMeshAgent.SetDestination(destination);
        }
        else
        {
            Debug.LogWarning($"Cannot set destination to {destination} for {m_MonsterName}. Path is not complete. {navMeshPath.status}");

            onFailCallback?.Invoke();
        }
    }

    public virtual void CloseToDestination()
    {
        if (Vector3.Distance(transform.position, m_NavMeshDestination) < 1f)
        {
            switch (m_CurrentState)
            {
                case MonsterState.Roaming:
                    StartRoaming();
                    break;
                case MonsterState.Stalking:
                    StartStalking();
                    break;
                case MonsterState.Fleeing:
                    StartFleeing();
                    break;
            }
        }
    }

    #endregion
}
