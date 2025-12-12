using FSMForUnity;
using UnityEngine;

/// <summary>
/// A state that makes a canvas group visible and enables
/// raycasting and interaction. This way we can ensure
/// that disabled states do not accidentally block the
/// active view.
/// </summary>
public class ShowPanelFSMState : IFSMState
{
    private readonly CanvasGroup canvasGroup;
    public ShowPanelFSMState(CanvasGroup canvasGroup)
    {
        this.canvasGroup = canvasGroup;
    }

    public void Enter()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Exit()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void Update(float delta)
    {

    }

    public void Destroy()
    {

    }
}
