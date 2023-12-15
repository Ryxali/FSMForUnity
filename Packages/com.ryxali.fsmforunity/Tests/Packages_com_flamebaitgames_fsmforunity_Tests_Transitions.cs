using System.Collections;
using System.Collections.Generic;
using FSMForUnity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class Packages_com_flamebaitgames_fsmforunity_Tests_Transitions
{
    [Test]
    public void TriggeredFSMTransition_ResetsWhenPassedThrough()
    {
        var transition = new TriggeredFSMTransition();

        Assert.False(transition.ShouldTransition());
        transition.Trigger();
        Assert.True(transition.ShouldTransition());
        transition.PassThrough();
        Assert.False(transition.ShouldTransition());
    }
}
