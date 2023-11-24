using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using FSMForUnity;

public class FSMDebugger : EditorWindow
{
    [MenuItem("Window/Analysis/FSM Debugger")]
    public static void ShowExample()
    {
        FSMDebugger wnd = GetWindow<FSMDebugger>();
        wnd.titleContent = new GUIContent("FSM Debugger");
    }
    private FSMDebuggerController controller;

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIMap_EditorWindow.Path);
        VisualElement rootElement = visualTree.Instantiate();
        root.Add(rootElement);
        controller = new FSMDebuggerController(rootElement);

        // A stylesheet can be added to a VisualElement.
        // The style will be applied to the VisualElement and all of its children.
        //var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.flamebaitgames.fsmforunity/Editor/FSMDebugger.uss");
        //VisualElement labelWithStyle = new Label("Hello World! With Style");
        //labelWithStyle.styleSheets.Add(styleSheet);
        //root.Add(labelWithStyle);
    }

    private void OnDestroy()
    {
        controller?.Destroy();
        controller = null;
    }

	private void OnInspectorUpdate()
	{
        // From docs OnInspectorUpdate is 10x per second
        const float InspectorFrameRate = 1f / 10f;
        controller?.Update(InspectorFrameRate);
        /*if (Selection.activeObject && DebuggingLinker.linkedMachines.TryGetValue(Selection.activeObject, out var machine))
        {
            if (machine != currentlyInspected)
            {
                currentlyInspected = machine;
                foldoutElement.Remove(rootProceduralElement);
                rootProceduralElement = GenerateElement(machine);
                foldoutElement.Add(rootProceduralElement);
            }
            // machine.DebugCurrent
            if (machine.DebugCurrent == null)
            {
                // foldoutElement.Add
            }
        }*/
	}

    private VisualElement GenerateElement(FSMMachine machine)
    {
        new UnityEngine.UIElements.Box();
        return null;
    }
}
