using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FSMForUnity;

/// <summary>
/// Periodically updates the supplied text component with a random
/// string, to emulate a loading progress.
/// </summary>
public class LoadingTextFSMState : IFSMState
{
    private readonly Text text;

    private readonly string[] entries = { "Loading", "Reticulating", "Buffering", "Aligning" };

    public LoadingTextFSMState(Text text)
    {
        this.text = text;
    }

    public void Enter()
    {
        var entry = entries[Random.Range(0, entries.Length)];
        text.text = entry;
    }

    public void Exit()
    {

    }

    public void Update(float delta)
    {
        if(Time.frameCount % 120 == 0)
        {
            var entry = entries[Random.Range(0, entries.Length)];
            text.text = entry;
        }
    }

    public void Destroy()
    {

    }
}
