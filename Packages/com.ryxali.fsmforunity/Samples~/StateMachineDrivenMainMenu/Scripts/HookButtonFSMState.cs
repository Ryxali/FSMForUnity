using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FSMForUnity;

/// <summary>
/// When this state is active, we map the click of the supplied button
/// so it triggers the supplied transition.
/// </summary>
public class HookButtonFSMState : IFSMState
{
    private readonly Button button;
    private readonly TriggeredFSMTransition settingsTransition;

    public HookButtonFSMState(Button button, TriggeredFSMTransition settingsTransition)
    {
        this.button = button;
        this.settingsTransition = settingsTransition;
    }

    public void Enter()
    {
        button.onClick.AddListener(settingsTransition.Trigger);
    }

    public void Exit()
    {
        button.onClick.RemoveListener(settingsTransition.Trigger);
    }

    public void Update(float delta)
    {

    }

    public void Destroy()
    {

    }
}
