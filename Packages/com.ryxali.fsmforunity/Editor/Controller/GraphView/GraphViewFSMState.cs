using FSMForUnity.Editor.IMGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

namespace FSMForUnity.Editor
{


    internal class GraphViewFSMState : IFSMState
    {
        private readonly DebuggerFSMStateData stateData;
        private readonly VisualElement container;

        private readonly RepeatingBackgroundElement graphCanvas;
        private readonly VisualTreeAsset graphNodeAsset;
        private readonly ObjectPool<VisualElement> graphNodePool;
        private readonly ObjectPool<ConnectionVisualElement> graphConnectionPool;

        private readonly List<VisualElement> graphNodes = new List<VisualElement>();
        private readonly List<ConnectionVisualElement> graphConnections = new List<ConnectionVisualElement>();

        private readonly Texture gridTexture;
        private readonly MachineGraph machineGraph;

        private bool isPanning;
        private Vector2 heldPosition;
        private float zoomLevel;
        private const float UnitConvert = 500f;

        public GraphViewFSMState(DebuggerFSMStateData stateData, VisualElement container)
        {
            this.stateData = stateData;
            this.container = container;
            gridTexture = IMGUIUtil.GenerateRepeatingGridTexture(128, 2, new Color(0.2f, 0.2f, 0.2f, 2f), new Color(0.6f, 0.6f, 0.6f, 1f));
            gridTexture.hideFlags = HideFlags.HideAndDontSave;
            graphCanvas = new RepeatingBackgroundElement(gridTexture);
            graphNodeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIMap_GraphView.GraphNodePath);
            graphNodePool = new ObjectPool<VisualElement>(() => graphNodeAsset.Instantiate().Q("Box"), elem => { elem.RemoveFromHierarchy(); });
            graphConnectionPool = new ObjectPool<ConnectionVisualElement>(() => new ConnectionVisualElement(), elem => { elem.RemoveFromHierarchy(); elem.Reset(); });
            machineGraph = new MachineGraph();
        }

        public void Enter()
        {
            machineGraph.Regenerate(stateData.currentlyInspecting);
            graphCanvas.Reset();
            container.Add(graphCanvas);
            int i = 0;
            foreach (var node in machineGraph.GetStates())
            {
                i++;
                var elem = graphNodePool.Take();
                graphNodes.Add(elem);
                container.Add(elem);
                InitializeNode(stateData.currentlyInspecting, elem, node, i);
            }
            foreach (var conn in machineGraph.GetTransitions())
            {
                var elem = graphConnectionPool.Take();
                graphConnections.Add(elem);
                container.Add(elem);
                var fromElem = graphNodes[conn.originIndex];
                var toElem = graphNodes[conn.destinationIndex];
                elem.Connect(graphNodes[conn.originIndex], graphNodes[conn.destinationIndex]);
            }
            foreach (var elem in graphNodes)
                elem.BringToFront();
            container.RegisterCallback<MouseDownEvent>(OnPanDown, TrickleDown.NoTrickleDown);
            container.RegisterCallback<MouseUpEvent>(OnPanUp, TrickleDown.NoTrickleDown);
            container.RegisterCallback<MouseMoveEvent>(OnPanDrag, TrickleDown.NoTrickleDown);
            container.RegisterCallback<WheelEvent>(OnZoom, TrickleDown.NoTrickleDown);
        }

        public void Exit()
        {
            container.UnregisterCallback<MouseDownEvent>(OnPanDown, TrickleDown.NoTrickleDown);
            container.UnregisterCallback<MouseUpEvent>(OnPanUp, TrickleDown.NoTrickleDown);
            container.UnregisterCallback<MouseMoveEvent>(OnPanDrag, TrickleDown.NoTrickleDown);
            container.UnregisterCallback<WheelEvent>(OnZoom, TrickleDown.NoTrickleDown);
            foreach (var elem in graphNodes)
            {
                elem.RemoveFromHierarchy();
                graphNodePool.Return(elem);
            }
            foreach (var elem in graphConnections)
            {
                elem.RemoveFromHierarchy();
                graphConnectionPool.Return(elem);
            }
            graphNodes.Clear();
            graphConnections.Clear();
            container.Remove(graphCanvas);
        }

        public void Update(float delta)
        {
        }

        void IFSMState.Destroy()
        {
            UnityEngine.Object.DestroyImmediate(gridTexture);
        }

        private void InitializeNode(DebugMachine debugMachine, VisualElement element, GraphNode node, int index)
        {
            element.style.left = new StyleLength(new Length(container.contentRect.width / 2 + node.position.x * UnitConvert));
            element.style.top = new StyleLength(new Length(container.contentRect.height / 2 + node.position.y * UnitConvert));
            //element.style.translate = new Translate(new Length(container.contentRect.width/2 + node.position.x * UnitConvert), new Length(container.contentRect.height / 2 + node.position.y * UnitConvert), 0f);
            element.Q<Label>(UIMap_GraphView.Title).text = index.ToString();
            element.Q<Label>(UIMap_GraphView.Subheading).text = debugMachine.GetStateName(node.state);
        }

        private void OnZoom(WheelEvent evt)
        {
            zoomLevel = Mathf.Clamp01(zoomLevel - evt.delta.y * 0.01f);
            graphCanvas.Zoom(Mathf.Lerp(1f, 10f, zoomLevel * zoomLevel), evt.localMousePosition);
        }

        private void OnPanDown(MouseDownEvent evt)
        {
            if (evt.button == 2)
            {
                isPanning = true;
                heldPosition = evt.mousePosition;
            }
        }
        private void OnPanUp(MouseUpEvent evt)
        {
            if (evt.button == 2)
            {
                isPanning = false;
            }
        }
        private void OnPanDrag(MouseMoveEvent evt)
        {
            if (isPanning)
            {
                var pos = evt.mousePosition;
                graphCanvas.Pan(pos - heldPosition);
                heldPosition = pos;
            }
        }
    }
}
