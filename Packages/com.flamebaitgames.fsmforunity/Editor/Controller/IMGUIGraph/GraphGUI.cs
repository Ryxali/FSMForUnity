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
            var diff = pointB - pointA;
            float a = Mathf.Rad2Deg * Mathf.Atan(diff.y / diff.x);
            if (diff.x < 0)
                a += 180;

            float angle = Vector2.SignedAngle(Vector2.up, pointB -pointA);
            GUIUtility.RotateAroundPivot(a, pointA);
            GUI.EndClip();
            var rect = new Rect (pointA.x, pointA.y + 8f, Vector2.Distance(pointA, pointB), lineWidth);
            GUI.DrawTexture(rect, lineTexture);
            GUIUtility.RotateAroundPivot(-a, pointA);
            GUI.BeginClip(clipRect);
            return false;
        }
    }
}
