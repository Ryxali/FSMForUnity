using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FSMForUnity.Editor.IMGUIGraph
{
    internal struct AnimatedNode
    {
        public Color color;
        public float updatePulse;
        public float enterPulse;
        public float exitPulse;
    }

    internal class GraphViewIMGUIFSMState : IFSMState, IMachineEventListener
    {

        private readonly DebuggerFSMStateData stateData;
        private readonly VisualElement container;
        private readonly VisualElement immediateGUIElement;

        private readonly GUISkin skin;
        private readonly Texture2D gridTexture;
        private readonly Texture2D lineTexture;

        private readonly MachineGraph machineGraph = new MachineGraph();
        private AnimatedNode[] animatedNodes;
        private Dictionary<IFSMState, int> nodeToIndex;
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
            skin.hideFlags = HideFlags.HideAndDontSave;
        }

        public void Enter()
        {
            machineGraph.Regenerate(stateData.currentlyInspecting);
            animatedNodes = new AnimatedNode[machineGraph.GetStates().Length];
            nodeToIndex = machineGraph.GetStates().Select((n, s) => (n, s)).ToDictionary(k => k.n.state, v => v.s);
            container.Add(immediateGUIElement);
            container.RegisterCallback<MouseDownEvent>(OnPanDown, TrickleDown.NoTrickleDown);
            container.RegisterCallback<MouseUpEvent>(OnPanUp, TrickleDown.NoTrickleDown);
            container.RegisterCallback<MouseMoveEvent>(OnPanDrag, TrickleDown.NoTrickleDown);
            container.RegisterCallback<WheelEvent>(OnZoom, TrickleDown.NoTrickleDown);
            stateData.eventBroadcaster.AddListener(this);
        }

        public void Exit()
        {
            stateData.eventBroadcaster.RemoveListener(this);
            container.UnregisterCallback<MouseDownEvent>(OnPanDown, TrickleDown.NoTrickleDown);
            container.UnregisterCallback<MouseUpEvent>(OnPanUp, TrickleDown.NoTrickleDown);
            container.UnregisterCallback<MouseMoveEvent>(OnPanDrag, TrickleDown.NoTrickleDown);
            container.UnregisterCallback<WheelEvent>(OnZoom, TrickleDown.NoTrickleDown);
            immediateGUIElement.RemoveFromHierarchy();
        }

        public void Update(float delta)
        {
            const float DecayTime = 0.3f;
            

            var decay = delta / DecayTime;
            for (int i = 0; i < animatedNodes.Length; i++)
            {
                var n = animatedNodes[i];
                n.color = Color.white;

                n.color = IMGUIUtil.Blend(UIMap_IMGUISkin.updateColor * n.updatePulse, n.color);
                n.color = IMGUIUtil.Blend(UIMap_IMGUISkin.enterColor * n.enterPulse, n.color);
                n.color = IMGUIUtil.Blend(UIMap_IMGUISkin.exitColor * n.exitPulse, n.color);

                n.enterPulse = Mathf.Max(0f, n.enterPulse - decay);
                n.exitPulse = Mathf.Max(0f, n.exitPulse - decay);
                n.updatePulse = Mathf.Max(0f, n.updatePulse - decay);
                animatedNodes[i] = n;
            }
            immediateGUIElement.MarkDirtyRepaint();
        }

        private void OnZoom(WheelEvent evt)
        {
            zoomLevel -= evt.delta.y * 0.05f;
            zoomLevel = Mathf.Clamp(zoomLevel, 0.1f, 10f);
        }

        private void OnPanDown(MouseDownEvent evt)
        {
            if (evt.button == 2)
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
                using (var imguiEvt = IMGUIEvent.GetPooled(e))
                {
                    immediateGUIElement.panel.visualTree.SendEvent(imguiEvt);
                }
            }
        }
        private void OnPanUp(MouseUpEvent evt)
        {
            if (evt.button == 2)
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
                using (var imguiEvt = IMGUIEvent.GetPooled(e))
                {
                    immediateGUIElement.panel.visualTree.SendEvent(imguiEvt);
                }
            }
        }
        private void OnPanDrag(MouseMoveEvent evt)
        {
            if (isPanning)
            {
                var pos = evt.mousePosition;
                panPosition += pos - heldPosition;
                heldPosition = pos;
                immediateGUIElement.MarkDirtyRepaint();
            }
        }

        void IMachineEventListener.OnTargetChanged(in DebugMachine machine)
        {
            // Debug.Log("Target Changed");
        }
        void IMachineEventListener.OnStateEnter(IFSMState state)
        {
            var node = animatedNodes[nodeToIndex[state]];
            node.enterPulse = 1f;
            animatedNodes[nodeToIndex[state]] = node;
        }
        void IMachineEventListener.OnStateEnter(IFSMState state, IFSMTransition through)
        {
            var node = animatedNodes[nodeToIndex[state]];
            node.enterPulse = 1f;
            animatedNodes[nodeToIndex[state]] = node;
        }
        void IMachineEventListener.OnStateExit(IFSMState state)
        {
            var node = animatedNodes[nodeToIndex[state]];
            node.exitPulse = 1f;
            animatedNodes[nodeToIndex[state]] = node;
        }
        void IMachineEventListener.OnStateExit(IFSMState state, IFSMTransition from)
        {
            var node = animatedNodes[nodeToIndex[state]];
            node.exitPulse = 1f;
            animatedNodes[nodeToIndex[state]] = node;
        }
        void IMachineEventListener.OnStateUpdate(IFSMState state)
        {
            var node = animatedNodes[nodeToIndex[state]];
            node.updatePulse = 1f;
            animatedNodes[nodeToIndex[state]] = node;
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
                using (IMGUIMatrixStack.Auto(GUI.matrix * Matrix4x4.Scale(Vector3.one * zoomLevel)))
                {
                    var stateRect = new Rect(panelRect.width / 2, panelRect.height / 2, 100, 100);

                    foreach (var transition in machineGraph.GetTransitions())
                    {
                        const float LineWidth = 10f;
                        var pointA = stateRect.position + transition.origin * BoxSpacing;
                        var pointB = stateRect.position + transition.destination * BoxSpacing;

                        GraphGUI.DrawConnection(panelRect, pointA, pointB, LineWidth, lineTexture);
                    }

                    var stateNodes = machineGraph.GetStates();
                    for (int i = 0; i < stateNodes.Length; i++)
                    {
                        var state = stateNodes[i];
                        var animNode = animatedNodes[i];
                        var color = UIMap_IMGUISkin.normalStateColor;
                        if (stateData.currentlyInspecting.TryGetActive(out var active) && active == state.state)
                            color = IMGUIUtil.Blend(UIMap_IMGUISkin.activeStateColor, color);
                        if (state.state == stateData.currentlyInspecting.DefaultState)
                            color = IMGUIUtil.Blend(UIMap_IMGUISkin.defaultStateColor, color);
                        color = animNode.color;//IMGUIUtil.Blend(animNode.color, color);
                        var clicked = GraphGUI.DrawStateNode(stateRect.position + state.position * BoxSpacing, 1f, stateData.currentlyInspecting.GetStateName(state.state), state.isDefault, color.gamma);

                        if (clicked)
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
            if (Application.isPlaying)
            {
                Object.Destroy(gridTexture);
                Object.Destroy(skin);
            }
            else
            {
                Object.DestroyImmediate(gridTexture);
                Object.DestroyImmediate(skin);
            }
        }
    }
}
