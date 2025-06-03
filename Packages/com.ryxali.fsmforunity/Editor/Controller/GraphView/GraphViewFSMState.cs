using FSMForUnity.Editor.IMGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

namespace FSMForUnity.Editor
{
    internal class GraphViewFSMState : IFSMState, IMachineEventListener
    {
        private const float NodeWidth = 75f;
        private const float NodeHeight = 50f;

        private readonly DebuggerFSMStateData stateData;
        private readonly VisualElement container;

        private readonly RepeatingBackgroundElement graphCanvas;
        private readonly VisualTreeAsset graphNodeAsset;
        private readonly ObjectPool<NodeVisualElement> graphNodePool;
        private readonly ObjectPool<ConnectionVisualElement> graphConnectionPool;

        private readonly List<NodeVisualElement> graphNodes = new List<NodeVisualElement>();
        private readonly List<ConnectionVisualElement> graphConnections = new List<ConnectionVisualElement>();

        private readonly Dictionary<IFSMState, NodeVisualElement> stateToElement = new Dictionary<IFSMState, NodeVisualElement>(EqualityComparer_IFSMState.constant);
        private readonly Dictionary<FromToTransition, ConnectionVisualElement> transitionToElement = new Dictionary<FromToTransition, ConnectionVisualElement>(EqualityComparer_FromToTransition.constant);
        private StateQueue stateQueue;

        private readonly VisualElement legendElement;

        private readonly MachineGraph machineGraph;

        private bool isPanning;
        private Vector2 heldPosition;
        private float zoomLevel;
        private const float UnitConvert = 150f;
        private float scaledUnitConvert;

        public GraphViewFSMState(DebuggerFSMStateData stateData, VisualElement container)
        {
            this.stateData = stateData;
            this.container = container;
            graphCanvas = new RepeatingBackgroundElement();
            graphNodeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIMap_GraphView.GraphNodePath);
            graphNodePool = new ObjectPool<NodeVisualElement>(() => graphNodeAsset.Instantiate().Q<NodeVisualElement>("Box"), elem => { elem.RemoveFromHierarchy(); });
            graphConnectionPool = new ObjectPool<ConnectionVisualElement>(() => new ConnectionVisualElement(), elem => { elem.RemoveFromHierarchy(); elem.Reset(); });
            machineGraph = new MachineGraph();

            legendElement = new VisualElement();
            legendElement.style.position = new StyleEnum<Position>(Position.Absolute);
            legendElement.style.right = new StyleLength(20);
            legendElement.style.bottom = new StyleLength(20);
            legendElement.Add(AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIMap_GraphView.LegendPath).Instantiate());
        }

        public void Enter()
        {
            machineGraph.Regenerate(stateData.currentlyInspecting);
            var connectionHandle = machineGraph.SolveConnectionAnchors(NodeWidth/ UnitConvert, NodeHeight / UnitConvert, out var edges, out var connectionCounts);
            scaledUnitConvert = UnitConvert / Mathf.Min(1f, machineGraph.MinFloatDistance());
            graphCanvas.Reset();
            graphCanvas.Zoom(Mathf.Lerp(1f, 10f, zoomLevel * zoomLevel), container.contentRect.size*0.5f);//
            container.Add(graphCanvas);
            int i = 0;
            foreach (var node in machineGraph.GetStates())
            {
                i++;
                var elem = graphNodePool.Take();
                graphNodes.Add(elem);
                container.Add(elem);
                InitializeNode(stateData.currentlyInspecting, elem, node, i);
                stateToElement.Add(node.state, elem);
            }
            connectionHandle.Complete();
            i = 0;
            foreach (var conn in machineGraph.GetTransitions())
            {
                var elem = graphConnectionPool.Take();
                graphConnections.Add(elem);
                container.Add(elem);
                var fromElem = graphNodes[conn.originIndex];
                var toElem = graphNodes[conn.destinationIndex];
                var fromI = i * 2;
                var toI = fromI + 1;
                elem.Connect(stateData.currentlyInspecting.GetTransitionName(conn.transition, conn.origin.state, conn.destination.state), graphNodes[conn.originIndex], edges[fromI], connectionCounts[fromI].index / (float)connectionCounts[fromI].count, graphNodes[conn.destinationIndex], edges[toI], connectionCounts[toI].index / (float)connectionCounts[toI].count);
                transitionToElement.Add(new FromToTransition { from = conn.origin.state, to = conn.destination.state, transition = conn.transition }, elem);
                transitionToElement.Add(new FromToTransition { from = null, to = conn.destination.state, transition = conn.transition }, elem);
                elem.Scale = graphCanvas.zoom;
                i++;
            }
            edges.Dispose();
            connectionCounts.Dispose();
            foreach (var elem in graphNodes)
                elem.BringToFront();
            container.Add(legendElement);
            container.RegisterCallback<MouseDownEvent>(OnPanDown, TrickleDown.NoTrickleDown);
            container.RegisterCallback<MouseUpEvent>(OnPanUp, TrickleDown.NoTrickleDown);
            container.RegisterCallback<MouseMoveEvent>(OnPanDrag, TrickleDown.NoTrickleDown);
            container.RegisterCallback<WheelEvent>(OnZoom, TrickleDown.NoTrickleDown);
            container.RegisterCallback<GeometryChangedEvent>(OnContainerDimensionsChange);
            container.RegisterCallback<NodeSelectedEvent>(OnNodeSelected);
            container.RegisterCallback<NodeDeselectedEvent>(OnNodeDeselected);
            stateData.eventBroadcaster.AddListener(this);
            if (stateData.currentlyInspecting.TryGetActive(out var active))
            {
                OnStateEnter(active);
                stateData.selectedState = active;
            }
            else
            {
                stateData.selectedState = null;
            }
        }

        private void OnNodeDeselected(NodeDeselectedEvent evt)
        {
            if (stateData.currentlyInspecting.TryGetActive(out var active))
            {
                stateData.selectedState = active;
            }
            else
            {
                stateData.selectedState = null;
            }
        }

        private void OnNodeSelected(NodeSelectedEvent evt)
        {
            var elem = evt.element;
            stateData.selectedState = stateToElement.First(k => k.Value == elem).Key;
        }

        public void Exit()
        {
            stateQueue.Reset();
            stateData.eventBroadcaster.RemoveListener(this);
            container.UnregisterCallback<GeometryChangedEvent>(OnContainerDimensionsChange);
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
            container.Remove(legendElement);
            container.Remove(graphCanvas);
            stateToElement.Clear();
            transitionToElement.Clear();
        }

        public void Update(float delta)
        {
            //gridTexture.SetPixels(gridTexture.GetPixels().Select(p => new Color(1 - p.r, 1 - p.g, 1 - p.b, p.a)).ToArray());
            //gridTexture.Apply();
        }

        void IFSMState.Destroy()
        {
            graphCanvas.Dispose();
        }

        private void InitializeNode(DebugMachine debugMachine, NodeVisualElement element, GraphNode node, int index)
        {
            var zoom = graphCanvas.zoom;
            element.style.left = new StyleLength(new Length(graphCanvas.offset.x + graphCanvas.zoom * container.contentRect.width / 2 + graphCanvas.zoom * node.position.x * scaledUnitConvert));
            element.style.top = new StyleLength(new Length(graphCanvas.offset.y + graphCanvas.zoom * container.contentRect.height / 2 + graphCanvas.zoom * node.position.y * scaledUnitConvert));
            //element.style.left = new StyleLength(new Length(graphCanvas.offset.x + graphCanvas.zoom * node.position.x * UnitConvert));
            //element.style.top = new StyleLength(new Length(graphCanvas.offset.y + graphCanvas.zoom * node.position.y * UnitConvert));
            element.style.width = new StyleLength(new Length(NodeWidth * zoom));
            element.style.height = new StyleLength(new Length(NodeHeight * zoom));
            element.style.translate = new StyleTranslate(new Translate(new Length(-50f, LengthUnit.Percent), new Length(-50f, LengthUnit.Percent), 0f));
            element.Title = index.ToString();
            element.Subheading = debugMachine.GetStateName(node.state);
        }

        private void OnZoom(WheelEvent evt)
        {
            zoomLevel = Mathf.Clamp01(zoomLevel - evt.delta.y * 0.01f);
            graphCanvas.Zoom(Mathf.Lerp(1f, 10f, zoomLevel * zoomLevel), evt.localMousePosition);

            var nodes = machineGraph.GetStates();
            for (int i = 0; i < graphNodes.Count; i++)
            {
                var elem = graphNodes[i];
                var node = nodes[i];
                var zoom = graphCanvas.zoom;
                elem.style.left = new StyleLength(new Length(graphCanvas.offset.x + graphCanvas.zoom * container.contentRect.width / 2 + graphCanvas.zoom * node.position.x * scaledUnitConvert));
                elem.style.top = new StyleLength(new Length(graphCanvas.offset.y + graphCanvas.zoom * container.contentRect.height / 2 + graphCanvas.zoom * node.position.y * scaledUnitConvert));
                elem.style.width = new StyleLength(new Length(NodeWidth * zoom));
                elem.style.height = new StyleLength(new Length(NodeHeight * zoom));
            }
            for (int i = 0; i < graphConnections.Count; i++)
            {
                var elem = graphConnections[i];
                var zoom = graphCanvas.zoom;
                elem.Scale = zoom;//Mathf.Max(zoomLevel,0.1f);
            }
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

                var nodes = machineGraph.GetStates();
                for (int i = 0; i < graphNodes.Count; i++)
                {
                    var elem = graphNodes[i];
                    var node = nodes[i];
                    elem.style.left = new StyleLength(new Length(graphCanvas.offset.x + graphCanvas.zoom * container.contentRect.width / 2 + graphCanvas.zoom * node.position.x * scaledUnitConvert));
                    elem.style.top = new StyleLength(new Length(graphCanvas.offset.y + graphCanvas.zoom * container.contentRect.height / 2 + graphCanvas.zoom * node.position.y * scaledUnitConvert));

                }
            }
        }

        public void OnTargetChanged(in DebugMachine machine)
        {
        }

        private void OnContainerDimensionsChange(GeometryChangedEvent evt)
        {
            var diff = (evt.oldRect.center - evt.newRect.center) - (evt.oldRect.position - evt.newRect.position);
            var pan = diff * graphCanvas.zoom;

            graphCanvas.Pan(pan);

            var nodes = machineGraph.GetStates();
            for (int i = 0; i < graphNodes.Count; i++)
            {
                var elem = graphNodes[i];
                var node = nodes[i];
                elem.style.left = new StyleLength(new Length(graphCanvas.offset.x + graphCanvas.zoom * container.contentRect.width / 2 + graphCanvas.zoom * node.position.x * scaledUnitConvert));
                elem.style.top = new StyleLength(new Length(graphCanvas.offset.y + graphCanvas.zoom * container.contentRect.height / 2 + graphCanvas.zoom * node.position.y * scaledUnitConvert));
            }
        }

        public void OnStateEnter(IFSMState state)
        {
            if (stateToElement.TryGetValue(state, out var elem))
            {
                stateQueue.Start(elem);
            }
        }

        public void OnStateEnter(IFSMState state, IFSMTransition through)
        {
            if (transitionToElement.TryGetValue(new FromToTransition { from = null, to = state, transition = through }, out var connElem))
            {
                connElem.Pulse();
            }

            if (stateToElement.TryGetValue(state, out var elem))
            {
                stateQueue.MoveNext(elem);
            }
            
        }

        public void OnStateExit(IFSMState state)
        {
        }

        public void OnStateExit(IFSMState state, IFSMTransition from)
        {
        }

        public void OnStateUpdate(IFSMState state)
        {
        }

        private struct StateQueue
        {
            private NodeVisualElement current, previous;

            public void Start(NodeVisualElement state)
            {
                current = state;
                current.AddToClassList("active");
            }

            public void Reset()
            {
                current?.RemoveFromClassList("active");
                previous?.RemoveFromClassList("was-active");
                current = previous = null;
            }

            public void MoveNext(NodeVisualElement next)
            {
                current?.RemoveFromClassList("active");
                previous?.RemoveFromClassList("was-active");
                previous = current;
                current = next;
                current.AddToClassList("active");
                previous.AddToClassList("was-active");
            }
        }
    }
}
