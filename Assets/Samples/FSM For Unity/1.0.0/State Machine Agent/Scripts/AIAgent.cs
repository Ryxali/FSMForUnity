using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSMForUnity;

/// <summary>
/// Here we define the behaviour for the agent. It has the following 3 states:
/// <list type="bullet">
/// <item>Idle</item>
/// <item>Approaching</item>
/// <item>Running Away</item>
/// </list>
/// The state depends on whether there is an active target for the AIAgent,
/// and how close that target is. If the target gets too close, it will run away.
/// If it's too far away, it will get closer.
/// If there is no active target, it will idle.
/// </summary>
[AddComponentMenu("")]
public class AIAgent : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    private FSMMachine fsm;
    private AIStateData stateData;

    void Awake()
    {
        // In this sample we define a state data that is shared
        // amongst states in the machine. This is one way to persist
        // data between states.
        stateData = new AIStateData(transform);

        var builder = FSMMachine.Build();

        // Add the three states
        var idleState = builder.AddState("Idle", new EmptyFSMState());
        var approachState = builder.AddState("Approaching", new ApproachFSMState(stateData));
        var runAwayState = builder.AddState("Fleeing", new RunAwayFSMState(stateData));

        // Here we use expressions to define whether we want to move to this state or not
        // By making it bidirectional and adding the existance of a target as part of the
        // transition condition we can ensure that the state is left if there is no target.
        // Doing so allows us to assume that the target != null in the state itself.
        builder.AddBidirectionalTransition(() => stateData.target && Vector3.Distance(stateData.target.position, stateData.myTransform.position) < 2f, idleState, runAwayState);
        builder.AddBidirectionalTransition(() => stateData.target && Vector3.Distance(stateData.target.position, stateData.myTransform.position) > 4f, idleState, approachState);
        builder.SetDebuggingInfo(name, this);

        fsm = builder.Complete();
    }

    IEnumerator TargetingLoop()
    {
        // Here we simply set and clear the target periodically
        while(true)
        {
            stateData.target = target;
            yield return new WaitForSeconds(10f);
            stateData.target = null;
            yield return new WaitForSeconds(2f);
        }
    }

    void Update()
    {
        fsm.Update(Time.deltaTime);
    }

    void OnEnable()
    {
        StartCoroutine(TargetingLoop());
        fsm.Enable();
    }

    void OnDisable()
    {
        fsm.Disable();
    }

    void OnDestroy()
    {
        fsm.Destroy();
        fsm = null;
    }
}
