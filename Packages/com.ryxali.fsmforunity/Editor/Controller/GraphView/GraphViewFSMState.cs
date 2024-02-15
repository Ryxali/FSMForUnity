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
        private readonly Texture gridTexture;
        private readonly MachineGraph machineGraph;

        private bool isPanning;
        private Vector2 heldPosition;
        private float zoomLevel;
        private const float UnitConvert = 250f;

        public GraphViewFSMState(DebuggerFSMStateData stateData, VisualElement container)
        {
            this.stateData = stateData;
            this.container = container;
            gridTexture = IMGUIUtil.GenerateRepeatingGridTexture(128, 2, new Color(0.2f, 0.2f, 0.2f, 2f), new Color(0.6f, 0.6f, 0.6f, 1f));
            gridTexture.hideFlags = HideFlags.HideAndDontSave;
            graphCanvas = new RepeatingBackgroundElement(gridTexture);
            graphNodeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIMap_GraphView.GraphNodePath);
            graphNodePool = new ObjectPool<VisualElement>(() => graphNodeAsset.Instantiate(), elem => { });
            graphConnectionPool = new ObjectPool<ConnectionVisualElement>(() => new ConnectionVisualElement(), elem => elem.Connect(default, default));
            machineGraph = new MachineGraph();
        }

        public void Enter()
        {
            machineGraph.Regenerate(stateData.currentlyInspecting);
            graphCanvas.Reset();
            container.Add(graphCanvas);
            foreach (var node in machineGraph.GetStates())
            {
                var elem = graphNodePool.Take();
                //var pos = elem.style.position;
                //pos.value = Position.Absolute;
                var translate = elem.style.translate;
                graphCanvas.Add(elem);
                var left = elem.style.left;
                var right = elem.style.right;
                translate.value = new Translate(new Length(container.contentRect.width/2 + node.position.x * UnitConvert), new Length(container.contentRect.height / 2 + node.position.y * UnitConvert), 0f);
                elem.style.translate = translate;
                //elem.transform.position = node.position;
            }
            foreach (var conn in machineGraph.GetTransitions())
            {
                var elem = graphConnectionPool.Take();
                graphCanvas.Add(elem);
                elem.Connect(new Rect(container.contentRect.size / 2 + conn.origin * UnitConvert, default), new Rect(container.contentRect.size / 2 + conn.destination * UnitConvert, default));
            }
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
            foreach (var elem in graphCanvas.Children())
            {
                if (elem is ConnectionVisualElement vElem)
                    graphConnectionPool.Return(vElem);
                else
                    graphNodePool.Return(elem);
            }
            graphCanvas.Clear();
            container.Remove(graphCanvas);
        }

        public void Update(float delta)
        {
        }

        void IFSMState.Destroy()
        {
            UnityEngine.Object.DestroyImmediate(gridTexture);
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
