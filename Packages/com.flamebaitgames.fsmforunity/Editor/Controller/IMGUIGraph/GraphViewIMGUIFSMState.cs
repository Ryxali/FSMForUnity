using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using FSMForUnity;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace FSMForUnity.Editor.IMGUIGraph
{
    internal class GraphViewIMGUIFSMState : IFSMState
    {
        private readonly DebuggerFSMStateData stateData;
        private readonly VisualElement container;
        private readonly VisualElement immediateGUIElement;

        private readonly Texture2D gridTexture;
        private readonly Texture2D lineTexture;

        private readonly MachineGraph machineGraph = new MachineGraph();

        private Vector2 panPosition;
        private Vector2 heldPosition;
        private bool isPanning;

        private float zoomLevel = 1f;
        private const float DefaultGridTiling = 32f;

        public GraphViewIMGUIFSMState(DebuggerFSMStateData stateData, VisualElement container)
        {
            this.stateData = stateData;
            this.container = container;
            immediateGUIElement = new IMGUIContainer(OnGUI);
            gridTexture = IMGUIUtil.GenerateRepeatingGridTexture(128, 2, new Color(0.2f, 0.2f, 0.2f, 2f), new Color(0.6f, 0.6f, 0.6f, 1f));
            gridTexture.hideFlags = HideFlags.HideAndDontSave;
            lineTexture = IMGUIUtil.GenerateRepeatingArrowTexture(96, 16, 2, new Color(0.8f, 0.8f, 0.8f, 0.8f));
            lineTexture.hideFlags = HideFlags.HideAndDontSave;
        }

        public void Enter()
        {
            machineGraph.Regenerate(stateData.currentlyInspecting);
            container.Add(immediateGUIElement);
            container.RegisterCallback<MouseDownEvent>(OnPanDown, TrickleDown.TrickleDown);
            container.RegisterCallback<MouseUpEvent>(OnPanUp, TrickleDown.TrickleDown);
            container.RegisterCallback<MouseMoveEvent>(OnPanDrag, TrickleDown.TrickleDown);
            container.RegisterCallback<WheelEvent>(OnZoom, TrickleDown.TrickleDown);

            // Generate nodes and connections
            // start with default state
            // position other nodes in a radius around default
            // generate transitions
            // use transitions as a spring force
            // try satisfy constraints
            // default state is only fixed node, rest can move
        }

        public void Exit()
        {
            container.UnregisterCallback<MouseDownEvent>(OnPanDown, TrickleDown.TrickleDown);
            container.UnregisterCallback<MouseUpEvent>(OnPanUp, TrickleDown.TrickleDown);
            container.UnregisterCallback<MouseMoveEvent>(OnPanDrag, TrickleDown.TrickleDown);
            container.UnregisterCallback<WheelEvent>(OnZoom, TrickleDown.TrickleDown);
            immediateGUIElement.RemoveFromHierarchy();
        }

        public void Update(float delta)
        {

        }

        private void OnZoom(WheelEvent evt)
        {
            zoomLevel -= evt.delta.y * 0.05f;
            zoomLevel = Mathf.Clamp(zoomLevel, 0.1f, 10f);
        }

        private void OnPanDown(MouseDownEvent evt)
        {
            if(evt.button == 2)
            {
                isPanning = true;
                heldPosition = evt.mousePosition;
            }
        }
        private void OnPanUp(MouseUpEvent evt)
        {
            if(evt.button == 2)
            {
                isPanning = false;
            }
        }
        private void OnPanDrag(MouseMoveEvent evt)
        {
            if(isPanning)
            {
                var pos = evt.mousePosition;
                panPosition += pos - heldPosition;
                heldPosition = pos;
            }
        }

        private void OnGUI()
        {
            var panelRect = new Rect(0, 0, container.resolvedStyle.width, container.resolvedStyle.height);
            GUI.BeginGroup(panelRect);
            var repeatingCoords = new Rect(0, 0, panelRect.width / DefaultGridTiling, panelRect.height / DefaultGridTiling);
            GUI.DrawTextureWithTexCoords(panelRect, gridTexture, repeatingCoords);

            const float BoxSpacing = 400f;
            float scaling = BoxSpacing;

            using (IMGUIMatrixStack.Auto(GUI.matrix * Matrix4x4.TRS(panPosition, Quaternion.identity, Vector3.one * zoomLevel)))
            {
                var stateRect = new Rect(panelRect.width / 2, panelRect.height / 2, 100, 100);

                foreach (var transition in machineGraph.GetTransitions())
                {
                    const float LineWidth = 10f;
                    var pointA = stateRect.position + transition.origin * BoxSpacing;
                    var pointB = stateRect.position + transition.destination * BoxSpacing;

                    GraphGUI.DrawConnection(panelRect, pointA, pointB, LineWidth, lineTexture);
                }

                foreach (var state in machineGraph.GetStates())
                {
                    GraphGUI.DrawStateNode(stateRect.position + state.position * scaling, 1f, state.state.ToString(), state.isDefault);
                }
            }

            GUI.EndGroup();
        }

        public void Destroy()
        {
            if(Application.isPlaying)
                Object.Destroy(gridTexture);
            else
                Object.DestroyImmediate(gridTexture);
        }
    }
}
