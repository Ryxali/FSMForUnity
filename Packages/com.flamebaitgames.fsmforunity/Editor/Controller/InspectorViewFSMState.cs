#if UNITY_2022_1_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;

namespace FSMForUnity
{
    internal struct InspectorEntry
    {
        public string name;
        public string type;
        public object value;
    }
    internal class InspectorViewFSMState : IFSMState
    {
        private static readonly Stack<System.Type> typeStack = new Stack<System.Type>();
        // We won't inspect deeper into these types


        private readonly DebuggerFSMStateData stateData;
        [FSMDebuggerHidden]
        private readonly VisualElement container;
        [FSMDebuggerHidden]
        private readonly VisualElement inspectorRoot;
        [FSMDebuggerHidden]
        private readonly VisualElementPool inspectorEntryPool;
        [FSMDebuggerHidden]
        private readonly Stack<(VisualElement parent, object obj, FieldInfo field, int depth)> fieldBuffer = new Stack<(VisualElement, object, FieldInfo, int)>();

        private readonly MultiColumnTreeView treeView;

        private readonly List<TreeViewItemData<InspectorEntry>> stateHierarchy = new List<TreeViewItemData<InspectorEntry>>();

        public InspectorViewFSMState(DebuggerFSMStateData stateData, VisualElement container)
        {
            this.stateData = stateData;
            this.container = container;
            inspectorEntryPool = new VisualElementPool(AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIMap_InspectorView.InspectorEntryPath));
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIMap_InspectorView.Path);
            inspectorRoot = visualTree.Instantiate();
            treeView = inspectorRoot.Q<MultiColumnTreeView>();

            treeView.SetRootItems(stateHierarchy);

            treeView.columns["name"].makeCell = () => new Label();
            treeView.columns["type"].makeCell = () => new Label();
            treeView.columns["value"].makeCell = () => new Label();

            treeView.columns["name"].bindCell = (elem, i) => (elem as Label).text = treeView.GetItemDataForIndex<InspectorEntry>(i).name;
            treeView.columns["type"].bindCell = (elem, i) => (elem as Label).text = treeView.GetItemDataForIndex<InspectorEntry>(i).type;
            treeView.columns["value"].bindCell = (elem, i) => (elem as Label).text = treeView.GetItemDataForIndex<InspectorEntry>(i).name;
        }

        public void Enter()
        {
            container.Add(inspectorRoot);
            //RefreshView();
            //var n0 = new TreeViewItemData<InspectorEntry>(1, new InspectorEntry
            //{
            //    name = "n0",
            //    type = "type",
            //    value = null
            //});
            //var n1 = new TreeViewItemData<InspectorEntry>(1, new InspectorEntry
            //{
            //    name = "n0",
            //    type = "type",
            //    value = null
            //}, new List<TreeViewItemData<InspectorEntry>> { n0 });
            var n2 = new TreeViewItemData<InspectorEntry>(1, new InspectorEntry
            {
                name = "n0",
                type = "type",
                value = null
            });
            //stateHierarchy.Add(n1);
            stateHierarchy.Add(n2);
            //treeView.Rebuild();
        }

        public void Exit()
        {
            inspectorRoot.RemoveFromHierarchy();
        }

        public void Update(float delta)
        {
            //RefreshView();
        }

        private void RefreshView()
        {
            inspectorEntryPool.ReturnAll();
            var currentState = stateData.selectedState;//stateData.currentlyInspecting.Debug_CurrentState;
            if (currentState == null)
            {
                inspectorRoot.Q<Label>(UIMap_InspectorView.StateName).text = "Inactive";
                return;
            }

            inspectorRoot.Q<Label>(UIMap_InspectorView.StateName).text = currentState.ToString();
            // Do tree iteration
            const int MaxDepth = 3;

            // Add initial items
            var stateRoot = inspectorRoot.Q(UIMap_InspectorView.StateRoot);
            fieldBuffer.Clear();
            foreach (var field in GetFields(currentState))
            {
                fieldBuffer.Push((stateRoot, currentState, field, 0));
            }
            while (fieldBuffer.Count > 0)
            {
                var entry = fieldBuffer.Pop();
                var value = entry.field.GetValue(entry.obj);
                var label = entry.field.Name;

                var elemFoldout = AddElement(entry.parent, label, value);

                if (!entry.field.FieldType.IsValueType && value != null && entry.depth < MaxDepth)
                {
                    if (ReflectionBlacklist.CanInspect(value.GetType()))
                    {
                        if (value is IEnumerable arr)
                        {
                            int i = 0;
                            foreach (var en in arr)
                            {
                                var arrFoldout = AddElement(elemFoldout, $"[{i}]", en);
                                if (en != null && ReflectionBlacklist.CanInspect(en.GetType()))
                                {
                                    foreach (var field in GetFields(en))
                                    {
                                        fieldBuffer.Push((arrFoldout, en, field, entry.depth + 2));
                                    }
                                }
                                i++;
                            }
                        }
                        else
                        {
                            foreach (var field in GetFields(value))
                            {
                                fieldBuffer.Push((elemFoldout, value, field, entry.depth + 1));
                            }
                        }
                    }
                }
            }
        }

        private VisualElement AddElement(VisualElement parent, string label, object value)
        {
            var elem = inspectorEntryPool.Take();

            var elemLabel = elem.Q<Foldout>(UIMap_InspectorView.InspectorEntryFoldout);
            var elemValue = elem.Q<Label>(UIMap_InspectorView.InspectorEntryValue);
            var elemFoldout = elem.Q<VisualElement>(UIMap_InspectorView.InspectorEntryFoldout);

            elemLabel.text = label;
            elemValue.text = value?.ToString() ?? "null";
            parent.Add(elem);
            return elemFoldout;
        }

        public void Destroy()
        {

        }

        private static FieldInfo[] GetFields(object obj)
        {
            if (obj == null) return System.Array.Empty<FieldInfo>();
            var list = new List<FieldInfo>();
            typeStack.Push(obj.GetType());
            while (typeStack.Count > 0)
            {
                var type = typeStack.Pop();
                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (field.GetCustomAttributes(typeof(FSMDebuggerHiddenAttribute), false)?.Length == 0)
                    {
                        list.Add(field);
                    }
                }
                if (type.BaseType != null && ReflectionBlacklist.CanInspect(type.BaseType))
                    typeStack.Push(type.BaseType);
            }
            typeStack.Clear();
            return list.ToArray();
        }
    }
}
#endif