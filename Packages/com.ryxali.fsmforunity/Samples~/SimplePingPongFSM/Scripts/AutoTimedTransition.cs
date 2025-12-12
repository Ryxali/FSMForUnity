using FSMForUnity;
using UnityEngine;

public class AutoTimedTransition : IFSMTransition
{
    private readonly float cooldown;
    private float next;

    public AutoTimedTransition(float cooldown)
    {
        this.cooldown = cooldown;
        next = 0f;
    }

    void IFSMTransition.PassThrough()
    {
        // Whenever we move through the transition, set so it can't happen again until time has elapsed beyond cooldown.
        // Note that we don't have any update method that we could increment time with, so we need to resort to calculating the time instead.
        next = Time.time + cooldown;
    }

    bool IFSMTransition.ShouldTransition()
    {
        return next <= Time.time;
    }
}