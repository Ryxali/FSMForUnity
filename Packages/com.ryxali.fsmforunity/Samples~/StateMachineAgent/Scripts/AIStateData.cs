using UnityEngine;


/// <summary>
/// Here we define the shared data for the AI
/// We could add more things here, like hunger,
/// health, mood, etc. to help drive our behaviour.
/// </summary>
public class AIStateData
{
    public readonly Transform myTransform;
    public readonly Material myMaterial;
    public readonly Color defaultColor;
    public Transform target;

    public AIStateData(Transform agentTransform, Material agentMaterial, Color defaultColor)
    {
        myTransform = agentTransform;
        myMaterial = agentMaterial;
        this.defaultColor = defaultColor;
    }
}
