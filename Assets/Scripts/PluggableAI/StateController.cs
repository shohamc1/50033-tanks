using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StateController : MonoBehaviour
{

    public State currentState;
    public EnemyStats enemyStats;
    public Transform eyes;
    public State remainState;

    [HideInInspector] public NavMeshAgent navMeshAgent;
    [HideInInspector] public TankShooting tankShooting;
    [HideInInspector] public List<Transform> wayPointList;
    [HideInInspector] public int nextWayPoint;
    [HideInInspector] public Transform chaseTarget;
    [HideInInspector] public float stateTimeElapsed;

    private bool aiActive;

    public delegate void gameEvent();
    public static event gameEvent TurnOnAlarm;
    public static event gameEvent TurnOffAlarm;


    void Awake()
    {
        tankShooting = GetComponent<TankShooting>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public void SetupAI(bool aiActivationFromTankManager, List<Transform> wayPointsFromTankManager)
    {
        wayPointList = wayPointsFromTankManager;
        aiActive = aiActivationFromTankManager;
        if (aiActive)
        {
            navMeshAgent.enabled = true;
        }
        else
        {
            navMeshAgent.enabled = false;
        }
    }

    public void TransitionToState(State nextState)
    {
        if (nextState == remainState) return;

        Debug.Log(nextState.name);

        string[] onStates = { "ChaseScanner", "AlertScanner", "ChaseChaser" };
        List<string> onStatesList = new List<string>(onStates);

        string[] offStates = { "PatrolScanner" };
        List<string> offStatesList = new List<string>(offStates);

        if (onStatesList.Contains(nextState.name) && !onStatesList.Contains(currentState.name))
        {
            TurnOnAlarm();
        }
        if (offStatesList.Contains(nextState.name) && !offStatesList.Contains(currentState.name))
        {
            TurnOffAlarm();
        }

        currentState = nextState;
        OnExitState();
    }

    public void DeathAlarm()
    {
        Debug.Log(currentState.name);
        if (currentState.name == "ChaseScanner" || currentState.name == "AlertScanner")
            TurnOffAlarm();
    }

    public bool CheckIfCountDownElapsed(float duration)
    {
        stateTimeElapsed += Time.deltaTime;
        return stateTimeElapsed >= duration;
    }

    void Update()
    {
        if (!aiActive) return;

        currentState.UpdateState(this);
    }

    void OnExitState()
    {
        stateTimeElapsed = 0;
    }

    void OnDrawGizmos()
    {
        if (currentState != null && eyes != null)
        {
            Gizmos.color = currentState.sceneGizmoColor;
            Gizmos.DrawWireSphere(eyes.position, enemyStats.lookSphereCastRadius);
        }
    }

}