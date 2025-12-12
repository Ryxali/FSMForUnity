using FSMForUnity;
using UnityEngine;

public class PingState : IFSMState
{
    public void Enter()
    {
        Debug.Log("Ping!");
    }

    public void Exit()
    {
    }

    public void Update(float delta)
    {
    }

    public void Destroy()
    {
        Debug.Log("Ping Destroyed");
    }
}
