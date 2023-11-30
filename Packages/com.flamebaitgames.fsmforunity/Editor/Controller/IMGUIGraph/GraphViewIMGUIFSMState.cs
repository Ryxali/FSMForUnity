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

        private readonly GUISkin skin;
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
            lineTexture = IMGUIUtil.GenerateRepeatingArrowTexture(96, 24, 4, new Color(0.8f, 0.8f, 0.8f, 0.8f));
            lineTexture.hideFlags = HideFlags.HideAndDontSave;
            skin = UIMap_IMGUISkin.CreateSkin();
        }

        public void Enter()
        {
            machineGraph.Regenerate(stateData.currentlyInspecting);
            container.Add(immediateGUIElement);
            container.RegisterCallback<MouseDownEvent>(OnPanDown, TrickleDown.NoTrickleDown);
            container.RegisterCallback<MouseUpEvent>(OnPanUp, TrickleDown.NoTrickleDown);
            container.RegisterCallback<MouseMoveEvent>(OnPanDrag, TrickleDown.NoTrickleDown);
            container.RegisterCallback<WheelEvent>(OnZoom, TrickleDown.NoTrickleDown);

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
            container.UnregisterCallback<MouseDownEvent>(OnPanDown, TrickleDown.NoTrickleDown);
            container.UnregisterCallback<MouseUpEvent>(OnPanUp, TrickleDown.NoTrickleDown);
            container.UnregisterCallback<MouseMoveEvent>(OnPanDrag, TrickleDown.NoTrickleDown);
            container.UnregisterCallback<WheelEvent>(OnZoom, TrickleDown.NoTrickleDown);
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
            else
            {
                var e = new Event
                {
                    button = evt.button,
                    mousePosition = evt.mousePosition,
                    type = EventType.MouseDown
                };
                using(var imguiEvt = IMGUIEvent.GetPooled(e))
                {
                    immediateGUIElement.panel.visualTree.SendEvent(imguiEvt);
                }
            }
        }
        private void OnPanUp(MouseUpEvent evt)
        {
            if(evt.button == 2)
            {
                isPanning = false;
            }
            else
            {
                var e = new Event
                {
                    button = evt.button,
                    mousePosition = evt.mousePosition,
                    type = EventType.MouseUp
                };
                using(var imguiEvt = IMGUIEvent.GetPooled(e))
                {
                    immediateGUIElement.panel.visualTree.SendEvent(imguiEvt);
                }
            }
        }
        private void OnPanDrag(MouseMoveEvent evt)
        {
            if(isPanning)
            {
                var pos = evt.mousePosition;
                panPosition += pos - heldPosition;
                heldPosition = pos;
                immediateGUIElement.MarkDirtyRepaint();
            }
            /*var e = new Event
            {
                button = evt.button,
                mousePosition = evt.mousePosition,
                type = EventType.MouseMove
            };
            using(var imguiEvt = IMGUIEvent.GetPooled(e))
            {
                immediateGUIElement.panel.visualTree.SendEvent(imguiEvt);
            }*/
        }

        private void OnGUI()
        {
            var s = GUI.skin;
            GUI.skin = skin;
            var panelRect = new Rect(0, 0, container.resolvedStyle.width, container.resolvedStyle.height);

            var repeatingCoords = new Rect(-panPosition.x, -panPosition.y, panelRect.width / DefaultGridTiling, panelRect.height / DefaultGridTiling);
            GUI.DrawTextureWithTexCoords(panelRect, gridTexture, repeatingCoords);

            const float BoxSpacing = 400f;

            using (IMGUIMatrixStack.Auto(GUI.matrix * Matrix4x4.Translate(panPosition)))
            {
                using(IMGUIMatrixStack.Auto(GUI.matrix * Matrix4x4.Scale(Vector3.one * zoomLevel)))
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
                        var color = UIMap_IMGUISkin.normalStateColor;
                        if (state.state == stateData.currentlyInspecting.DebugCurrent)
                            color = UIMap_IMGUISkin.activeStateColor;
                        else if (state.state == stateData.currentlyInspecting.Debug_DefaultState)
                            color = UIMap_IMGUISkin.defaultStateColor;
                        var clicked = GraphGUI.DrawStateNode(stateRect.position + state.position * BoxSpacing, 1f, state.state.ToString(), state.isDefault, color);

                        if(clicked)
                        {
                            stateData.selectedState = state.state;
                        }
                    }
                }
            }
            GUI.skin = s;
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
