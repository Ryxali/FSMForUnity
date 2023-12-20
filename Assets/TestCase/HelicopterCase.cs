using FSMForUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelicopterCase : MonoBehaviour
{
    private FSMMachine machine;

    private void Awake()
    {
        var builder = FSMMachine.Build();

        var idle = builder.AddState("Idle", new EmptyFSMState());
        var takeOff = builder.AddState("Take-off", new EmptyFSMState());
        var initalize = builder.AddState("Initialize", new EmptyFSMState());
        var search = builder.AddState("Search", new EmptyFSMState());
        var identify = builder.AddState("Identify", new EmptyFSMState());
        var hoverAbove = builder.AddState("Hover above", new EmptyFSMState());
        var descendToGrasp = builder.AddState("Descend to grasp", new EmptyFSMState());
        var grasp = builder.AddState("Grasp", new EmptyFSMState());
        var ascend = builder.AddState("Ascend", new EmptyFSMState());
        var transport = builder.AddState("Transport", new EmptyFSMState());
        var descendToDrop = builder.AddState("Descend to drop", new EmptyFSMState());
        var drop = builder.AddState("Drop", new EmptyFSMState());
        var returnToSearch = builder.AddState("Return to search", new EmptyFSMState());
        var returnToBase = builder.AddState("Return to base", new EmptyFSMState());
        var land = builder.AddState("Land", new EmptyFSMState());
        var reacquireTarget = builder.AddState("Reacquire target", new EmptyFSMState());
        var emergencyLand = builder.AddState("Emergency land", new EmptyFSMState());
        const float Chance = 0.995f;

        // 1
        builder.AddLambdaTransition("Battery full", () => Random.value > Chance, idle, takeOff);
        // 2
        builder.AddLambdaTransition("At height", () => Random.value > Chance, takeOff, initalize);
        // 3
        builder.AddLambdaTransition("Systems bad", () => Random.value > Chance, initalize, land);
        builder.AddLambdaTransition("Systems okay", () => Random.value > Chance, initalize, search);
        // 4
        builder.AddLambdaTransition("End of path", () => Random.value > Chance, search, returnToBase);
        builder.AddLambdaTransition("Target found", () => Random.value > Chance, search, identify);
        // 5
        builder.AddLambdaTransition("Identified", () => Random.value > Chance, identify, hoverAbove);
        // 6
        builder.AddLambdaTransition("Target lost", () => Random.value > Chance, hoverAbove, search);
        builder.AddLambdaTransition("Above target", () => Random.value > Chance, hoverAbove, descendToGrasp);
        // 7
        builder.AddLambdaTransition("Too long", () => Random.value > Chance, descendToGrasp, search);
        builder.AddLambdaTransition("At object height", () => Random.value > Chance, descendToGrasp, grasp);
        // 8
        builder.AddLambdaTransition("Grabbed", () => Random.value > Chance, grasp, ascend);
        // 9
        builder.AddLambdaTransition("Target dropped", () => Random.value > Chance, ascend, reacquireTarget);
        builder.AddLambdaTransition("At transport height", () => Random.value > Chance, ascend, transport);
        // 10
        builder.AddLambdaTransition("Target dropped", () => Random.value > Chance, transport, reacquireTarget);
        builder.AddLambdaTransition("Above drop site", () => Random.value > Chance, transport, descendToDrop);
        // 11
        builder.AddLambdaTransition("Dropped early", () => Random.value > Chance, descendToDrop, search);
        builder.AddLambdaTransition("At drop height", () => Random.value > Chance, descendToDrop, drop);
        // 12
        builder.AddLambdaTransition("Targets remaining", () => Random.value > Chance, drop, returnToSearch);
        builder.AddLambdaTransition("All targets found", () => Random.value > Chance, drop, returnToBase);
        // 13
        builder.AddLambdaTransition("At search height", () => Random.value > Chance, returnToSearch, search);
        // 14
        builder.AddLambdaTransition("Above base", () => Random.value > Chance, returnToBase, land);
        // 15
        builder.AddLambdaTransition("Landed", () => Random.value > Chance, land, idle);
        // 16
        builder.AddLambdaTransition("Target lost", () => Random.value > Chance, reacquireTarget, search);
        builder.AddLambdaTransition("Target found", () => Random.value > Chance, reacquireTarget, hoverAbove);
        // 17
        builder.AddLambdaTransition("Landed", () => Random.value > Chance, emergencyLand, idle);

        //foreach (var from in new[] { takeOff, initalize, search, identify, hoverAbove, descendToGrasp, grasp, ascend, transport, descendToDrop, drop, returnToSearch, returnToBase, land, reacquireTarget })
        //{
        //    // [2,3,...16]
        //    builder.AddLambdaTransition("Rotor loss", () => Random.value > Chance, from, emergencyLand);
        //}

        //foreach (var from in new[] { takeOff, initalize, search, identify, hoverAbove, descendToGrasp, grasp, ascend, transport, descendToDrop, drop, returnToSearch, reacquireTarget })
        //{
        //    // [2,3,...13, 16]
        //    builder.AddLambdaTransition("Low battery", () => Random.value > Chance, from, emergencyLand);
        //}

        machine = builder.Complete();
    }

    private void OnEnable()
    {
        machine.Enable();
    }

    private void Update()
    {
        machine.Update(Time.deltaTime);
    }

    private void OnDisable()
    {
        machine.Disable();
    }

    private void OnDestroy()
    {
        machine.Destroy();
        machine = null;
    }
}
