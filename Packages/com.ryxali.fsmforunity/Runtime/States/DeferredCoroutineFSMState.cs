using FSMForUnity;
using System.Collections;

public class DeferredCoroutineFSMState : CoroutineFSMState
{
    private readonly System.Func<IEnumerator> enter;
    private readonly System.Func<DeltaTime, IEnumerator> enter2;
    private readonly System.Action exit;

    public DeferredCoroutineFSMState(System.Func<IEnumerator> enter, System.Action exit)
    {
        this.enter = enter;
        this.exit = exit;
    }
    public DeferredCoroutineFSMState(System.Func<DeltaTime, IEnumerator> enter, System.Action exit)
    {
        this.enter2 = enter;
        this.exit = exit;
    }

    protected override IEnumerator Enter(DeltaTime deltaTime)
    {
        if (enter != null)
        {
            return enter.Invoke();
        }
        else if (enter2 != null)
        {
            return enter2.Invoke(deltaTime);
        }
        else
        {
            return null;
        }
    }

    protected override void Exit()
    {
        exit?.Invoke();
    }

    
}
