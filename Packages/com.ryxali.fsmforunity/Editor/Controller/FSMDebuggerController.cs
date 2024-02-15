using FSMForUnity.Editor.IMGUI;
using UnityEngine;
using UnityEngine.UIElements;

namespace FSMForUnity.Editor
{
    internal class FSMDebuggerController
    {
        private readonly FSMMachine fsm;
        private readonly VisualElement root;
        private readonly DebuggerFSMStateData stateData;

        public FSMDebuggerController(VisualElement root)
        {
            stateData = new DebuggerFSMStateData();
            var builder = FSMMachine.Build();

            var listView = new MachineListViewFSMState(stateData, root.Q(UIMap_EditorWindow.ListView));

            var graphBuilder = FSMMachine.Build();
            var graphNoSelected = graphBuilder.AddState("Display Graph", new EmptyFSMState());
#if UNITY_2022_1_OR_NEWER
            var graphSelected = graphBuilder.AddState("No Graph", new GraphViewFSMState(stateData, root.Q(UIMap_EditorWindow.GraphView)));
#else
            var graphSelected = graphBuilder.AddState("No Graph", new GraphViewIMGUIFSMState(stateData, root.Q(UIMap_EditorWindow.GraphView)));
#endif
            graphBuilder.AddBidirectionalTransition(() => stateData.currentlyInspecting.IsValid, graphNoSelected, graphSelected);
            graphBuilder.SetDebuggingInfo("FSM Debugger Graph", null);

            var inspectorBuilder = FSMMachine.Build();
            var inspectorNoSelected = inspectorBuilder.AddState("No Inspector", new EmptyFSMState());
#if UNITY_2022_1_OR_NEWER
            var inspectorSelected = inspectorBuilder.AddState("Show Inspector", new InspectorViewFSMState(stateData, root.Q(UIMap_EditorWindow.InspectorView)));
#else
            var inspectorSelected = inspectorBuilder.AddState("Show Inspector (2022+ only)", new EmptyFSMState());
#endif
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

        public void OnSelectionChanged(Object selected)
        {
            if (DebuggingLinker.TryGetLinkedMachineForObject(selected, out var machine))
            {
                stateData.wantToInspectNext = machine;
            }
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

