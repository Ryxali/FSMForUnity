using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSMForUnity;

/// <summary>
/// A state that makes a canvas group visible and enables
/// raycasting and interaction. This way we can ensure
/// that disabled states do not accidentally block the
/// active view.
/// </summary>
public class ApproachFSMState : IFSMState
{
    private readonly AIStateData stateData;

    public ApproachFSMState(AIStateData stateData)
    {
        this.stateData = stateData;
    }

    public void Enter()
    {

    }

    public void Exit()
    {

    }

    public void Update(float delta)
    {
        var fromTo = stateData.target.position - stateData.myTransform.position;
        stateData.myTransform.position += fromTo.normalized * 2f * Time.deltaTime;
    }

    public void Destroy()
    {

    }
}
