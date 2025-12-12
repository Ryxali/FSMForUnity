using FSMForUnity;
using UnityEngine;

/// <summary>
/// Creates and hosts the machine that flips between a ping and a pong state.
/// It will move between the states at a rate defined by cooldown. Due to the nature of
/// value types, the cooldown cannot be changed once the machine has been created.
/// While simple, this sample illustrates issues with stateful transitions, as during
/// fsm.Enable() we see it enter the ping state, then immediately move to the pong state.
/// </summary>
[AddComponentMenu("")]
public class PingPongHost : MonoBehaviour
{
    private FSMMachine fsm;
    [SerializeField]
    private float cooldown = 1f;

    private void Awake()
    {
        var builder = FSMMachine.Build();

        var pingState = builder.AddState(new PingState());
        var pongState = builder.AddState(new PongState());

        var transition = new AutoTimedTransition(cooldown);

        builder.AddTransition(transition, pingState, pongState);
        builder.AddTransition(transition, pongState, pingState);

        fsm = builder.Complete();
    }

    private void OnEnable()
    {
        fsm.Enable();
    }

    private void OnDisable()
    {
        fsm.Disable();
    }

    private void Update()
    {
        fsm.Update(Time.deltaTime);
    }

    private void OnDestroy()
    {
        fsm.Destroy();
        fsm = null;
    }
}
