using System.Collections;
using System.Collections.Generic;
using FSMForUnity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class Packages_com_flamebaitgames_fsmforunity_Tests_Builder
{
    [Test]
    public void CanAddState()
    {
        var builder = FSMMachine.Build();

        Assert.DoesNotThrow(() => builder.AddState(new EmptyFSMState()));
    }

    [Test]
    public void AddingSameStateTwice_Throws_ArgumentException()
    {
        var builder = FSMMachine.Build();
        var state = new EmptyFSMState();
        Assert.DoesNotThrow(() => builder.AddState(state));
        Assert.Throws<System.ArgumentException>(() => builder.AddState(state));
    }

    [Test]
    public void AddingNullState_Throws_ArgumentNullException()
    {
        var builder = FSMMachine.Build();
        Assert.Throws<System.ArgumentNullException>(() => builder.AddState(null));
    }

    [Test]
    public void CanSetDefaultState()
    {
        var builder = FSMMachine.Build();
        var state = builder.AddState(new EmptyFSMState());
        Assert.DoesNotThrow(() => builder.SetDefaultState(state));
    }

    [Test]
    public void SettingDefaultState_WithStateNotAddedToMachine_Throws_ArgumentException()
    {
        var builder = FSMMachine.Build();
        var state = new EmptyFSMState();
        Assert.Throws<System.ArgumentException>(() => builder.SetDefaultState(state));
    }

    [Test]
    public void CanAddFromToTransition()
    {
        var builder = FSMMachine.Build();
        var from = builder.AddState(new EmptyFSMState());
        var to = builder.AddState(new EmptyFSMState());
        Assert.DoesNotThrow(() => builder.AddTransition(AlwaysFSMTransition.constant, from, to));
    }

    [Test]
    public void AddingFromToTransition_WithAnyArgNull_Throws_ArgumentNullException()
    {
        var builder = FSMMachine.Build();
        var from = builder.AddState(new EmptyFSMState());
        var to = builder.AddState(new EmptyFSMState());
        Assert.Throws<System.ArgumentNullException>(() => builder.AddTransition(null, from, to));
        Assert.Throws<System.ArgumentNullException>(() => builder.AddTransition(AlwaysFSMTransition.constant, null, to));
        Assert.Throws<System.ArgumentNullException>(() => builder.AddTransition(AlwaysFSMTransition.constant, from, null));
    }

    [Test]
    public void AddingFromToTransition_BetweenAnyStateOutsideMachine_Throws_ArgumentException()
    {
        var builder = FSMMachine.Build();
        var inMachineState = builder.AddState(new EmptyFSMState());
        var otherState = new EmptyFSMState();
        Assert.Throws<System.ArgumentException>(() => builder.AddTransition(AlwaysFSMTransition.constant, inMachineState, otherState));
        Assert.Throws<System.ArgumentException>(() => builder.AddTransition(AlwaysFSMTransition.constant, otherState, inMachineState));
    }

    [Test]
    public void AddingFromToTransition_AddingDuplicateTransition_Throws_ArgumentException()
    {
        var builder = FSMMachine.Build();
        var from = builder.AddState(new EmptyFSMState());
        var to = builder.AddState(new EmptyFSMState());
        Assert.DoesNotThrow(() => builder.AddTransition(AlwaysFSMTransition.constant, from, to));
        Assert.Throws<System.ArgumentException>(() => builder.AddTransition(AlwaysFSMTransition.constant, from, to));
    }

    [Test]
    public void CanAddAnyTransition()
    {
        var builder = FSMMachine.Build();
        var to = builder.AddState(new EmptyFSMState());
        Assert.DoesNotThrow(() => builder.AddAnyTransition(AlwaysFSMTransition.constant, to));
    }

    [Test]
    public void AddingAnyTransition_WithAnyArgNull_Throws_ArgumentNullException()
    {
        var builder = FSMMachine.Build();
        var to = builder.AddState(new EmptyFSMState());
        Assert.Throws<System.ArgumentNullException>(() => builder.AddAnyTransition(null, to));
        Assert.Throws<System.ArgumentNullException>(() => builder.AddAnyTransition(AlwaysFSMTransition.constant, null));
    }

    [Test]
    public void AddingAnyTransition_ToAnyStateOutsideMachine_Throws_ArgumentException()
    {
        var builder = FSMMachine.Build();
        var otherState = new EmptyFSMState();
        Assert.Throws<System.ArgumentException>(() => builder.AddAnyTransition(AlwaysFSMTransition.constant, otherState));
    }

    [Test]
    public void AddingAnyTransition_AddingDuplicateTransition_Throws_ArgumentException()
    {
        var builder = FSMMachine.Build();
        var to = builder.AddState(new EmptyFSMState());
        Assert.DoesNotThrow(() => builder.AddAnyTransition(AlwaysFSMTransition.constant, to));
        Assert.Throws<System.ArgumentException>(() => builder.AddAnyTransition(AlwaysFSMTransition.constant, to));
    }
}
