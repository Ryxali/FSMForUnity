using UnityEditor;
using UnityEngine;

internal class EditorPrefColor
{
    public Color value { get => GetColor(); set => SetColor(value); }

    private readonly string path;
    private readonly int defaultValue;

    public EditorPrefColor(string path, Color defaultColor)
    {
        this.path = path;
        defaultValue = ColorToInt(defaultColor);
    }

    public Color GetColor()
    {
        var colorInt = EditorPrefs.GetInt(path, defaultValue);
        return IntToColor(colorInt);
    }

    public void SetColor(Color color)
    {
        EditorPrefs.SetInt(path, ColorToInt(color));
        Debug.Log($"SetInt {path} = {ColorToInt(color)}");
    }

    private static int ColorToInt(Color color)
    {
        
        Color32 col32 = color;
        return (col32.a << 24) + (col32.b << 16) + (col32.g << 8) + col32.r;
    }

    private static Color32 IntToColor(int colorInt)
    {
        var a = (colorInt >> 24) & 0xff;
        var b = (colorInt >> 16) & 0xff;
        var g = (colorInt >> 8) & 0xff;
        var r = colorInt & 0xff;
        return new Color32((byte)r, (byte)g, (byte)b, (byte)a);
    }

    public void Reset()
    {
        SetColor(IntToColor(defaultValue));
    }
}
