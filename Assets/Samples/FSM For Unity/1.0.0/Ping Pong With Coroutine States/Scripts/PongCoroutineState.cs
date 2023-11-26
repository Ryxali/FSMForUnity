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

    protected override IEnumerator Enter(DeltaTime deltaTime)
    {
        yield return Pang(deltaTime);

        Debug.Log("Pong");
        var t = 0f;
        while(t < 1)
        {
            // delta time is accessible via CoroutineFSMState
            t += deltaTime;
            yield return null;
        }
        yield return Pang(deltaTime);
        // When the coroutine is done we want to trigger the transition
        // to the other state
        transition.Trigger();
    }

    IEnumerator Pang(DeltaTime deltaTime)
    {
        Debug.Log("Pang");
        var t = 0f;
        while(t < 1)
        {
            // delta time is accessible via CoroutineFSMState
            t += deltaTime;
            yield return null;
        }
        yield return Pyng(deltaTime);
    }

    IEnumerator Pyng(DeltaTime deltaTime)
    {
        Debug.Log("Pyng");
        var t = 0f;
        while(t < 1)
        {
            // delta time is accessible via CoroutineFSMState
            t += deltaTime;
            yield return null;
        }
    }

    protected override void Exit()
    {
    }

    protected override void Destroy()
    {

    }
}
