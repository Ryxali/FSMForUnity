using System.Collections;
using System.Collections.Generic;
using FSMForUnity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class Packages_com_flamebaitgames_fsmforunity_Tests_Machine
{
    [Test]
    public void Enable_Enters_State()
    {
        var builder = FSMMachine.Build();

        bool entered = false;
        var defaultState = builder.AddState(new LambdaFSMState(enter: () => entered = true));

        var fsm = builder.Complete();

        fsm.Enable();
        Assert.True(entered);
    }

    [Test]
    public void Disable_Exits_State()
    {
        var builder = FSMMachine.Build();

        bool exited = false;
        var defaultState = builder.AddState(new LambdaFSMState(exit: () => exited = true));

        var fsm = builder.Complete();

        fsm.Enable();
        fsm.Disable();
        Assert.True(exited);
    }

    [Test]
    public void Update_Updates_State()
    {
        var builder = FSMMachine.Build();

        bool updated = false;
        var defaultState = builder.AddState(new LambdaFSMState(update: (dt) => updated = true));

        var fsm = builder.Complete();

        fsm.Enable();
        fsm.Update(0f);
        Assert.True(updated);
    }

    [Test]
    public void Update_DoesNotUpdateState_WhenDisabled()
    {
        var builder = FSMMachine.Build();

        bool updated = false;
        var defaultState = builder.AddState(new LambdaFSMState(update: (dt) => updated = true));

        var fsm = builder.Complete();

        fsm.Update(0f);
        Assert.False(updated);
    }

    [Test]
    public void Enable_WithTransitions_Enters_BothStates()
    {
        var builder = FSMMachine.Build();

        bool s0_entered = false;
        bool s1_entered = false;
        var state0 = builder.AddState(new LambdaFSMState(enter: () => s0_entered = true));
        var state1 = builder.AddState(new LambdaFSMState(enter: () => s1_entered = true));
        builder.AddTransition(AlwaysFSMTransition.constant, state0, state1);
        var fsm = builder.Complete();

        fsm.Enable();
        Assert.True(s0_entered, "State 0 wasn't entered");
        Assert.True(s1_entered, " State 1 wasn't entered");
    }

    [Test]
    public void Update_WithTransitions_Updates_IntendedState()
    {
        var builder = FSMMachine.Build();

        bool s1_updated = false;
        var state0 = builder.AddState(new EmptyFSMState());
        var state1 = builder.AddState(new LambdaFSMState(update: (dt) => s1_updated = true));
        builder.AddTransition(AlwaysFSMTransition.constant, state0, state1);
        var fsm = builder.Complete();

        fsm.Enable();
        fsm.Update(0f);
        Assert.True(s1_updated, "State 1 wasn't updated");
    }

    [Test]
    public void Update_WithTransitions_Transitions_ToIntendedState()
    {
        var builder = FSMMachine.Build();

        bool s1_updated = false;
        var state0 = builder.AddState(new EmptyFSMState());
        var state1 = builder.AddState(new LambdaFSMState(update: (dt) => s1_updated = true));
        var triggeredTransition = new TriggeredFSMTransition();
        builder.AddTransition(triggeredTransition, state0, state1);
        var fsm = builder.Complete();

        fsm.Enable();
        triggeredTransition.Trigger();
        fsm.Update(0f);
        Assert.True(s1_updated, "State 1 wasn't updated");
    }
}
