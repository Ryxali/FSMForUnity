using UnityEditor;
using UnityEngine;

internal class EditorPrefColor
{
    public Color value { get; set; }

    private readonly string path;
    private readonly int defaultValue;

    public EditorPrefColor(string path, Color defaultColor)
    {
        Debug.Log(defaultColor);
        this.path = path;
        defaultValue = ColorToInt(defaultColor);
    }

    public Color GetColor()
    {
        var colorInt = EditorPrefs.GetInt(path, defaultValue);
        var a = (colorInt >> 24) & 0xff;
        var b = (colorInt >> 16) & 0xff;
        var g = (colorInt >> 8) & 0xff;
        var r = colorInt & 0xff;
        return new Color32((byte)r, (byte)g, (byte)b, (byte)a);
    }

    public void SetColor(Color color)
    {
        EditorPrefs.SetInt(path, ColorToInt(color));
    }

    private int ColorToInt(Color color)
    {
        var col32 = (Color32)color;
        return col32.a << 24 + col32.b << 16 + col32.g << 8 + col32.r;
    }
}
