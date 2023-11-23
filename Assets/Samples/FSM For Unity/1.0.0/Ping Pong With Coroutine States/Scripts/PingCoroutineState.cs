using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSMForUnity;

public class PingCoroutineState : CoroutineFSMState
{
    private readonly TriggeredFSMTransition transition;

    public PingCoroutineState(TriggeredFSMTransition transition)
    {
        this.transition = transition;
    }

    protected override IEnumerator Enter(DeltaTime deltaTime)
    {
        Debug.Log("Ping");
        var t = 0f;
        while(t < 1)
        {
            // delta time is accessible via CoroutineFSMState´
            t += deltaTime;
            yield return null;
        }
        // When the coroutine is done we want to trigger the transition
        // to the other state
        transition.Trigger();
    }

    protected override void Exit()
    {
    }

    protected override void Destroy()
    {

    }
}
