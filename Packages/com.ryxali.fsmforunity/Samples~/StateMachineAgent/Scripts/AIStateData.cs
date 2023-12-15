using UnityEngine;


/// <summary>
/// Here we define the shared data for the AI
/// We could add more things here, like hunger,
/// health, mood, etc. to help drive our behaviour.
/// </summary>
public class AIStateData
{
    public readonly Transform myTransform;
    public Transform target;

    public AIStateData(Transform agentTransform)
    {
        myTransform = agentTransform;
    }
}
