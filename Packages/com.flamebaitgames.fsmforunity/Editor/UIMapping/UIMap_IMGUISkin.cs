using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

internal static class UIMap_IMGUISkin
{
    public const string NodeLabelStyle = "node-label";

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
