using UnityEngine;

internal static class FSMForUnityPreferences
{
    public static EditorPrefColor gridView_backgroundColor = new EditorPrefColor("fsmforunity-gridview-bgcolor", new Color32(0x14, 0x14, 0x14, 0xff));
    public static EditorPrefColor gridView_gridColor = new EditorPrefColor("fsmforunity-gridview-fgcolor", new Color32(0x1c, 0x1c, 0x1c, 0xff));
    public static EditorPrefToggle settings_showEditorMachines = new EditorPrefToggle("fsmforunity-show-editor-machines", false);

    internal static void ResetAllForPackage()
    {
        gridView_backgroundColor.Reset();
        gridView_gridColor.Reset();
        settings_showEditorMachines.Reset();
    }
}
