using FSMForUnity;
using UnityEngine;

/// <summary>
/// A state that makes a canvas group visible and enables
/// raycasting and interaction. This way we can ensure
/// that disabled states do not accidentally block the
/// active view.
/// </summary>
public class RunAwayFSMState : IFSMState
{
    private readonly AIStateData stateData;

    public RunAwayFSMState(AIStateData stateData)
    {
        this.stateData = stateData;
    }

    public void Enter()
    {
        stateData.myMaterial.SetColor("_Color", Color.yellow);
    }

    public void Exit()
    {
        stateData.myMaterial.SetColor("_Color", stateData.defaultColor);
    }

    public void Update(float delta)
    {
        var fromTo = stateData.target.position - stateData.myTransform.position;
        stateData.myTransform.position += -fromTo.normalized * 2f * Time.deltaTime;
    }

    public void Destroy()
    {

    }
}
