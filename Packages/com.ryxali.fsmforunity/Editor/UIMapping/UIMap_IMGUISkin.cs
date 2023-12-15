using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FSMForUnity.Editor.IMGUI
{
    internal static class UIMap_IMGUISkin
    {
        public const string NodeLabelStyle = "node-label";

        public static readonly Color normalStateColor = Color.white;
        public static readonly Color activeStateColor = new Color(0.35f, 0.75f, 0.35f, 0.5f).linear;
        public static readonly Color defaultStateColor = new Color(0.75f, 0.75f, 0.35f, 0.5f).linear;

        public static readonly Color updateColor = activeStateColor;
        public static readonly Color enterColor = new Color(0.35f, 1f, 0.35f, 1f).linear;
        public static readonly Color exitColor = new Color(1f, 0.35f, 0.35f, 1f).linear;

        public static GUISkin CreateSkin()
        {
            var skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);

            var copy = Object.Instantiate(skin);

            var list = new List<GUIStyle>(copy.customStyles);

            var nodeLabel = new GUIStyle(copy.label);
            nodeLabel.name = NodeLabelStyle;
            nodeLabel.alignment = TextAnchor.LowerCenter;
            list.Add(nodeLabel);

            copy.customStyles = list.ToArray();
            return copy;
        }
    }
}
