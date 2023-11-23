using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSMForUnity;

/// <summary>
/// A coroutine state that will fade a canvas group so it becomes invisible
/// When complete it will trigger a transition so we can move to a more
/// interactive state for the user.
/// </summary>
public class FadeOutFSMState : CoroutineFSMState
{
    private readonly CanvasGroup canvasGroup;
    private readonly TriggeredFSMTransition transition;

    public FadeOutFSMState(CanvasGroup canvasGroup, TriggeredFSMTransition transition)
    {
        this.canvasGroup = canvasGroup;
        this.transition = transition;
    }

    protected override IEnumerator Enter(DeltaTime deltaTime)
    {
        var t = 0f;
        // Fade alpha to 0 over 1 second
        while(t < 1f)
        {
            canvasGroup.alpha = 1 - t;
            t += deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0f;
        transition.Trigger();
    }

    protected override void Exit()
    {
    }

    protected override void Destroy()
    {

    }
}
