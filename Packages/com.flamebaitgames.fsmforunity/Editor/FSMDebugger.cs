using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using FSMForUnity;

public class FSMDebugger : EditorWindow
{
    [MenuItem("Window/UI Toolkit/FSMDebugger")]
    public static void ShowExample()
    {
        FSMDebugger wnd = GetWindow<FSMDebugger>();
        wnd.titleContent = new GUIContent("FSMDebugger");
    }

    private VisualElement foldoutElement;
    private FSMMachine currentlyInspected;
    private VisualElement rootProceduralElement;

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.flamebaitgames.fsmforunity/Editor/com.FlamebaitGames.fsmforunity_StateView.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);

        foldoutElement = root.Q("state-foldout");

        // A stylesheet can be added to a VisualElement.
        // The style will be applied to the VisualElement and all of its children.
        //var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.flamebaitgames.fsmforunity/Editor/FSMDebugger.uss");
        //VisualElement labelWithStyle = new Label("Hello World! With Style");
        //labelWithStyle.styleSheets.Add(styleSheet);
        //root.Add(labelWithStyle);
    }

	private void OnInspectorUpdate()
	{
        if (Selection.activeObject && DebuggingLinker.linkedMachines.TryGetValue(Selection.activeObject, out var machine))
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
        }
	}

    private VisualElement GenerateElement(FSMMachine machine)
    {
        new UnityEngine.UIElements.Box();
        return null;
    }
}