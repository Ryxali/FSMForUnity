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
            machineGraph.Regenerate(stateData.currentlyInspecting);

        }

        private void OnGUI()
        {
            var panelRect = new Rect(0, 0, container.resolvedStyle.width, container.resolvedStyle.height);
            GUI.BeginGroup(panelRect);
            var repeatingCoords = new Rect(0, 0, panelRect.width / DefaultGridTiling, panelRect.height / DefaultGridTiling);
            GUI.DrawTextureWithTexCoords(panelRect, gridTexture, repeatingCoords);

            const float BoxSpacing = 300f;

            var stateRect = new Rect(panelRect.width/2, panelRect.height/2, 100, 100);

            foreach(var transition in machineGraph.GetTransitions())
            {
                const float LineWidth = 2f;
                var pointA = stateRect.position + transition.origin * BoxSpacing;
                var pointB = stateRect.position + transition.destination * BoxSpacing;
                var diff = pointB - pointA;
                float a = Mathf.Rad2Deg * Mathf.Atan(diff.y / diff.x);
                if (diff.x < 0)
                    a += 180;

                float angle = Vector2.SignedAngle(Vector2.up, pointB -pointA);// Mathf.Atan2 (pointB.y - pointA.y, pointB.x - pointA.x) * 180f / Mathf.PI;
                GUIUtility.RotateAroundPivot(a, pointA);
                GUI.EndClip();
                var rect = new Rect (pointA.x, pointA.y, Vector2.Distance(pointA, pointB), LineWidth);
                GUI.DrawTexture(rect, lineTexture);
                GUIUtility.RotateAroundPivot(-a, pointA);
                GUI.BeginClip(panelRect);
            }

            foreach(var state in machineGraph.GetStates())
            {
                var r = stateRect;
                r.x += state.position.x * BoxSpacing - 50;
                r.y += state.position.y * BoxSpacing - 50;
                if(state.isDefault){

                    GUI.Box(r, "(Default) " + state.state.ToString() );
                }else{

                    GUI.Box(r, state.state.ToString());
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
