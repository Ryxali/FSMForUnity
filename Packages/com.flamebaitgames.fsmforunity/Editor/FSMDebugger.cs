using FSMForUnity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class FSMDebugger : EditorWindow
{
    private FSMDebuggerController controller;

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIMap_EditorWindow.Path);
        VisualElement rootElement = visualTree.Instantiate();
        // the TemplateContainer doesn't flex, so we force it to.
        rootElement.style.flexGrow = new StyleFloat(1f);
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
        if (!UpdateOften())
        {
            // From docs OnInspectorUpdate is 10x per second
            const float InspectorFrameRate = 1f / 10f;
            controller?.Update(InspectorFrameRate);
        }
    }
    private void OnSelectionChange()
    {
        // propagate selection changed event so the controller
        // can automatically find and select the element (if exists).
        controller?.OnSelectionChanged(Selection.activeObject);
    }

    private void Update()
    {
        if (UpdateOften())
        {
            // When in play mode we update more often for smoother animations
            controller?.Update(Time.deltaTime);
        }
    }

    private bool UpdateOften() => hasFocus && EditorApplication.isPlaying;

    [MenuItem("Window/Analysis/FSM Debugger")]
    public static void ShowExample()
    {
        FSMDebugger wnd = GetWindow<FSMDebugger>();
        wnd.titleContent = new GUIContent("FSM Debugger");
    }
}
