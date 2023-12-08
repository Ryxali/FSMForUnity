using FSMForUnity;
using System.Collections;
using UnityEngine;

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
[RequireComponent(typeof(Renderer))]
public class AIAgent : MonoBehaviour
{
    [SerializeField]
    private Transform target;
    [SerializeField]
    private Color color = Color.gray;

    private FSMMachine fsm;
    private AIStateData stateData;

    void Awake()
    {
        // In this sample we define a state data that is shared
        // amongst states in the machine. This is one way to persist
        // data between states.
        Material materialInstance = GetComponent<Renderer>().material;
        materialInstance.SetColor("_Color", color);
        stateData = new AIStateData(transform, materialInstance, color);

        FSMMachine.IBuilder builder = FSMMachine.Build();

        // Add the three states
        IFSMState idleState = builder.AddState("Idle", new EmptyFSMState());
        IFSMState approachState = builder.AddState("Approaching", new ApproachFSMState(stateData));
        IFSMState runAwayState = builder.AddState("Fleeing", new RunAwayFSMState(stateData));

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
        while (true)
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
