using FSMForUnity;
using UnityEngine;

public class PongState : IFSMState
{
    public void Enter()
    {
        Debug.Log("Pong!");
    }

    public void Exit()
    {
    }

    public void Update(float delta)
    {
    }

    public void Destroy()
    {
        Debug.Log("Pong Destroyed");
    }
}