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
            var graphNoSelected = graphBuilder.AddState(new EmptyFSMState());
            var graphSelected = graphBuilder.AddState(new GraphViewIMGUIFSMState(stateData, root.Q(UIMap_EditorWindow.GraphView)));
            graphBuilder.AddBidirectionalTransition(() => stateData.currentlyInspecting != null, graphNoSelected, graphSelected);
            graphBuilder.SetDebuggingInfo("FSM Debugger Graph", null);

            var inspectorBuilder = FSMMachine.Build();
            var inspectorNoSelected = inspectorBuilder.AddState(new EmptyFSMState());
            var inspectorSelected = inspectorBuilder.AddState(new InspectorViewFSMState(stateData, root.Q(UIMap_EditorWindow.InspectorView)));
            inspectorBuilder.AddBidirectionalTransition(() => stateData.currentlyInspecting != null, inspectorNoSelected, inspectorSelected);
            inspectorBuilder.SetDebuggingInfo("FSM Debugger Inspector", null);

            var selectionBuilder = FSMMachine.Build();
            var noSelection = selectionBuilder.AddState(new EmptyFSMState());
            var newSelection = selectionBuilder.AddLambdaState(enter: () => stateData.currentlyInspecting = stateData.wantToInspectNext);
            var haveSelection = selectionBuilder.AddParallelState
            (
                new LambdaFSMState(enter: () => stateData.selectedState = null, update: default, exit: default),
                new SubstateFSMState(graphBuilder.Complete()),
                new SubstateFSMState(inspectorBuilder.Complete())
            );
            selectionBuilder.AddTransition(() => stateData.currentlyInspecting == null, haveSelection, noSelection);
            selectionBuilder.AddTransition(() => stateData.wantToInspectNext != null, noSelection, newSelection);
            selectionBuilder.AddTransition(() => stateData.currentlyInspecting != null, newSelection, haveSelection);
            selectionBuilder.AddTransition(() => stateData.wantToInspectNext != stateData.currentlyInspecting, haveSelection, newSelection);
            selectionBuilder.SetDebuggingInfo("FSM Debugger Selection", null);

            builder.AddParallelState(
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
    
