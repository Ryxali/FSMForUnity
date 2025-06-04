using UnityEditor;

internal class EditorPrefToggle
{
    public bool value { get => EditorPrefs.GetBool(path, defaultValue); set { SetWithoutNotify(value); onChanged(value); } }

    public event System.Action<bool> onChanged = delegate { };

    private readonly string path;
    private readonly bool defaultValue;

    public EditorPrefToggle(string path, bool defaultValue)
    {
        this.path = path;
        this.defaultValue = defaultValue;
    }

    internal void SetWithoutNotify(bool value)
    {
        EditorPrefs.SetBool(path, value);
    }

    public void Reset()
    {
        value = defaultValue;
    }
}
