using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSMForUnity;

public class PongCoroutineState : CoroutineFSMState
{
    private readonly TriggeredFSMTransition transition;

    public PongCoroutineState(TriggeredFSMTransition transition)
    {
        this.transition = transition;
    }

    protected override IEnumerator OnEnter()
    {
        Debug.Log("Pong");
        var t = 0f;
        while(t < 1)
        {
            // delta time is accessible via CoroutineFSMState
            t += deltaTime;
            yield return null;
        }
    }

    protected override void OnCoroutineEnd()
    {
        // When the coroutine is done we want to trigger the transition
        // to the other state
        transition.Trigger();
    }

    public override void Destroy()
    {

    }
}
