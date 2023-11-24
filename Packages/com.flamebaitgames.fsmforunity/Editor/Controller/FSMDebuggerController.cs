using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using FSMForUnity;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

internal class FSMDebuggerController
{
    private readonly FSMMachine fsm;
    private readonly VisualElement root;

    public FSMDebuggerController(VisualElement root)
    {
        var stateData = new DebuggerFSMStateData();
        var builder = FSMMachine.Build();

        var listView = new ListViewFSMState(stateData, root.Q(UIMap_EditorWindow.ListView));

        var graphView = new EmptyFSMState(); // TODO implement

        var inspectorBuilder = FSMMachine.Build();
        var inspectorNoSelected = inspectorBuilder.AddState(new EmptyFSMState());
        var inspectorSelected = inspectorBuilder.AddState(new InspectorViewFSMState(stateData, root.Q(UIMap_EditorWindow.InspectorView)));
        inspectorBuilder.AddBidirectionalTransition(() => stateData.currentlyInspecting != null, inspectorNoSelected, inspectorSelected);
        inspectorBuilder.SetDebuggingInfo("FSM Debugger Inspector", null);

        builder.AddState(new ParallelFSMState(listView, graphView, new SubstateFSMState(inspectorBuilder.Complete())));
        builder.SetDebuggingInfo("FSM Debugger", null);
        fsm = builder.Complete();
        fsm.Enable();
    }

    public void Update(float deltaTime)
    {
        fsm.Update(deltaTime);
    }

    public void Destroy()
    {
        fsm.Destroy();
    }
}
