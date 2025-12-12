using FSMForUnity;
using System.Collections;
using UnityEngine;

/// <summary>
/// A coroutine state that will fade a canvas group so it becomes visible
/// When complete it will trigger a transition so we can move to a more
/// interactive state for the user.
/// </summary>
public class FadeInFSMState : CoroutineFSMState
{
    private readonly CanvasGroup canvasGroup;
    private readonly TriggeredFSMTransition transition;

    public FadeInFSMState(CanvasGroup canvasGroup, TriggeredFSMTransition transition)
    {
        this.canvasGroup = canvasGroup;
        this.transition = transition;
    }

    protected override IEnumerator Enter(DeltaTime deltaTime)
    {
        var t = 0f;

        while (t < 1f)
        {
            canvasGroup.alpha = t;
            t += deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1f;
        transition.Trigger();
    }

    protected override void Exit()
    {
    }

    protected override void Destroy()
    {

    }
}
