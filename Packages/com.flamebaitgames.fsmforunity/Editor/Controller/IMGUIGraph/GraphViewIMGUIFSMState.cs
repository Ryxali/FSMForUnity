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
        private float zoomLevel;
        private const float DefaultGridTiling = 32f;

        public GraphViewIMGUIFSMState(DebuggerFSMStateData stateData, VisualElement container)
        {
            this.stateData = stateData;
            this.container = container;
            immediateGUIElement = new IMGUIContainer(OnGUI);
            gridTexture = IMGUIUtil.GenerateRepeatingGridTexture(128, 2, new Color(0.2f, 0.2f, 0.2f, 2f), new Color(0.6f, 0.6f, 0.6f, 1f));
            gridTexture.hideFlags = HideFlags.HideAndDontSave;
            lineTexture = new Texture2D(1, 1);
            lineTexture.SetPixel(0,0, Color.white);
            lineTexture.Apply();
            lineTexture.hideFlags = HideFlags.HideAndDontSave;
        }

        public void Enter()
        {
            machineGraph.Regenerate(stateData.currentlyInspecting);
            container.Add(immediateGUIElement);

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
            immediateGUIElement.RemoveFromHierarchy();
        }

        public void Update(float delta)
        {

        }

        private void OnGUI()
        {
            var panelRect = new Rect(0, 0, container.resolvedStyle.width, container.resolvedStyle.height);
            GUI.BeginGroup(panelRect);
            var repeatingCoords = new Rect(0, 0, panelRect.width / DefaultGridTiling, panelRect.height / DefaultGridTiling);
            GUI.DrawTextureWithTexCoords(panelRect, gridTexture, repeatingCoords);

            const float BoxSpacing = 400f;

            var stateRect = new Rect(panelRect.width/2, panelRect.height/2, 100, 100);

            foreach(var transition in machineGraph.GetTransitions())
            {
                const float LineWidth = 2f;
                var pointA = stateRect.position + transition.origin * BoxSpacing;
                var pointB = stateRect.position + transition.destination * BoxSpacing;

                GraphGUI.DrawConnection(panelRect, pointA, pointB, LineWidth, lineTexture);
            }

            foreach(var state in machineGraph.GetStates())
            {
                GraphGUI.DrawStateNode(stateRect.position + state.position * BoxSpacing, 1f, state.state.ToString(), state.isDefault);
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
