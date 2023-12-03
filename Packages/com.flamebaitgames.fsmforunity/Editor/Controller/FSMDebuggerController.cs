using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using FSMForUnity.Editor.IMGUIGraph;

namespace FSMForUnity
{
    internal class FSMDebuggerController
    {
        private readonly FSMMachine fsm;
        private readonly VisualElement root;

        public FSMDebuggerController(VisualElement root)
        {
            var stateData = new DebuggerFSMStateData();
            var builder = FSMMachine.Build();

            var listView = new ListViewFSMState(stateData, root.Q(UIMap_EditorWindow.ListView));

            var graphBuilder = FSMMachine.Build();
            var graphNoSelected = graphBuilder.AddState("Display Graph", new EmptyFSMState());
            var graphSelected = graphBuilder.AddState("No Graph", new GraphViewIMGUIFSMState(stateData, root.Q(UIMap_EditorWindow.GraphView)));
            graphBuilder.AddBidirectionalTransition(() => stateData.currentlyInspecting.IsValid, graphNoSelected, graphSelected);
            graphBuilder.SetDebuggingInfo("FSM Debugger Graph", null);

            var inspectorBuilder = FSMMachine.Build();
            var inspectorNoSelected = inspectorBuilder.AddState("Show Inspector", new EmptyFSMState());
            var inspectorSelected = inspectorBuilder.AddState("No Inspector", new InspectorViewFSMState(stateData, root.Q(UIMap_EditorWindow.InspectorView)));
            inspectorBuilder.AddBidirectionalTransition(() => stateData.currentlyInspecting.IsValid, inspectorNoSelected, inspectorSelected);
            inspectorBuilder.SetDebuggingInfo("FSM Debugger Inspector", null);

            var selectionBuilder = FSMMachine.Build();
            var noSelection = selectionBuilder.AddState("No Selection", new EmptyFSMState());
            var newSelection = selectionBuilder.AddLambdaState("New Selection", enter: () => stateData.currentlyInspecting = stateData.wantToInspectNext);
            var haveSelection = selectionBuilder.AddParallelState
            (
                name: "Have Selection",
                new LambdaFSMState(enter: () => stateData.selectedState = null),
                new LambdaFSMState(enter: () => stateData.eventBroadcaster.SetTarget(stateData.currentlyInspecting), update: (dt) => stateData.eventBroadcaster.Poll()),
                new SubstateFSMState(graphBuilder.Complete()),
                new SubstateFSMState(inspectorBuilder.Complete())
            );
            selectionBuilder.AddLambdaTransition(() => !stateData.currentlyInspecting.IsValid, haveSelection, noSelection);
            selectionBuilder.AddLambdaTransition(() => stateData.wantToInspectNext.IsValid, noSelection, newSelection);
            selectionBuilder.AddLambdaTransition(() => stateData.currentlyInspecting.IsValid, newSelection, haveSelection);
            selectionBuilder.AddLambdaTransition(() => stateData.wantToInspectNext != stateData.currentlyInspecting, haveSelection, newSelection);
            selectionBuilder.SetDebuggingInfo("FSM Debugger Selection", null);

            builder.AddParallelState
            (
                name: "Main",
                listView,
                new SubstateFSMState(selectionBuilder.Complete())
            );
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
}
    
