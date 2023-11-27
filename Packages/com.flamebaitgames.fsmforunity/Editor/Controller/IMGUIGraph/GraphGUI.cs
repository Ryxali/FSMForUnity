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
    public static class GraphGUI
    {

        private static GUIStyle nodeLabelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.LowerCenter
        };

        public static bool DrawStateNode(Vector2 center, float scale, string label, bool isDefaultState)
        {
            var size = new Vector2(200f, 150f);
            var box = new Rect(center.x-size.x/2f, center.y-size.y/2f, size.x * scale, size.y * scale);
            var clicked = GUI.Button(box, GUIContent.none);
            GUILayout.BeginArea(IMGUIUtil.PadRect(box, 5f));
            if(isDefaultState)
                GUILayout.Label("(Default)", nodeLabelStyle);
            GUILayout.Label(label, nodeLabelStyle);
            GUILayout.EndArea();
            return clicked;
        }

        public static bool DrawConnection(Rect clipRect, Vector2 pointA, Vector2 pointB, float lineWidth, Texture2D lineTexture)
        {
            // clamp
            var diff = pointB - pointA;
            float a = Mathf.Rad2Deg * Mathf.Atan(diff.y / diff.x);
            if (diff.x < 0)
                a += 180;

            const float RepeatRate = 50f;
            var repeatRate = RepeatRate;
            using (IMGUIMatrixStack.Auto(GUI.matrix * Matrix4x4.Translate(pointA)))
            {
                using (IMGUIMatrixStack.Auto(GUI.matrix * Matrix4x4.Rotate(Quaternion.Euler(0, 0, a))))
                {
                    var rect = new Rect(0f, 8f, Vector2.Distance(pointA, pointB), lineWidth);
                    var repeatingCoords = new Rect(0, 0, rect.width / repeatRate, 1);
                    GUI.DrawTextureWithTexCoords(rect, lineTexture, repeatingCoords);
                }
            }
            return false;
        }
    }
}
