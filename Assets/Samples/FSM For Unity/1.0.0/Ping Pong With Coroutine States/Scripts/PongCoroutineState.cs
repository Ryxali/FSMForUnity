using FSMForUnity;
using System.Collections;
using UnityEngine;

public class PongCoroutineState : CoroutineFSMState
{
    private readonly TriggeredFSMTransition transition;

    public PongCoroutineState(TriggeredFSMTransition transition)
    {
        this.transition = transition;
    }

    protected override IEnumerator Enter(DeltaTime deltaTime)
    {
        Debug.Log("Pong");
        // We can wait via this WaitForSeconds call
        yield return WaitForSeconds(1f);
        // When the coroutine is done we want to trigger the transition
        // to the other state
        transition.Trigger();
    }

    protected override void Exit()
    {
    }
}
