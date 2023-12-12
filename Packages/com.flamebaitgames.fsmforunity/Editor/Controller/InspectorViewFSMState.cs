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
    internal class ReflectionCache
    {
        private readonly Stack<System.Type> typeStack = new Stack<System.Type>(32);
        private readonly Dictionary<System.Type, FieldInfo[]> cachedReflection = new Dictionary<System.Type, FieldInfo[]>(4096);
        private readonly Dictionary<System.Type, string> cachedFieldNames = new Dictionary<System.Type, string>();

        private const BindingFlags GetFieldsFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public FieldInfo[] GetFields(System.Type type)
        {
            if (cachedReflection.TryGetValue(type, out var fields))
            {
                return fields;
            }
            else
            {
                var f = GetFieldsDeep(type);
                cachedReflection.Add(type, f);
                return f;
            }
        }

        public string GetName(System.Type type)
        {
            if (cachedFieldNames.TryGetValue(type, out var name))
            {
                return name;
            }
            else
            {
                var n = type.Name;
                cachedFieldNames.Add(type, n);
                return n;
            }
        }

        private FieldInfo[] GetFieldsDeep(System.Type rootType)
        {
            var list = new List<FieldInfo>();
            typeStack.Push(rootType);
            while (typeStack.Count > 0)
            {
                var type = typeStack.Pop();
                foreach (var field in type.GetFields(GetFieldsFlags))
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

    internal class StateHierarchy
    {
        private const int MaxDepth = 6;

        private readonly List<TreeViewItemData<InspectorEntry>> rootItems = new List<TreeViewItemData<InspectorEntry>>();

        private readonly Stack<List<TreeViewItemData<InspectorEntry>>> listPool = new Stack<List<TreeViewItemData<InspectorEntry>>>(256);

        private readonly ReflectionCache reflectionCache = new ReflectionCache();

        private readonly Stack<StackData> refreshStack = new Stack<StackData>(32);

        private int idCounter;

        private struct StackData
        {
            public int depth;
            public object element;
            public List<TreeViewItemData<InspectorEntry>> fields;
        }

        public void Refresh(object rootObject)
        {
            idCounter = 1;
            rootItems.Clear();
            refreshStack.Push(new StackData
            {
                depth = 0,
                element = rootObject,
                fields = rootItems
            });
            while (refreshStack.Count > 0)
            {
                var elem = refreshStack.Pop();

                var elemType = elem.element.GetType();
                foreach (var field in reflectionCache.GetFields(elemType))
                {
                    var value = field.GetValue(elem.element);

                    UnityEngine.Profiling.Profiler.BeginSample(reflectionCache.GetName(elemType));

                    if (ShouldInspectChildren(field, value, elem.depth))
                    {
                        if (value is IEnumerable enumerable)
                        {
                            var index = 0;
                            var arrayFields = new List<TreeViewItemData<InspectorEntry>>();
                            for (var en = enumerable.GetEnumerator(); en.MoveNext(); index++)
                            {
                                var indexValue = en.Current;
                                var childFields = new List<TreeViewItemData<InspectorEntry>>();
                                if (indexValue != null)
                                {
                                    refreshStack.Push(new StackData
                                    {
                                        element = indexValue,
                                        depth = elem.depth + 2,
                                        fields = childFields
                                    });
                                }
                                arrayFields.Add(new TreeViewItemData<InspectorEntry>(idCounter++, new InspectorEntry
                                {
                                    name = $"[{index}]",
                                    type = string.Empty,
                                    value = indexValue
                                }, childFields));
                            }
                            elem.fields.Add(new TreeViewItemData<InspectorEntry>(idCounter++, new InspectorEntry
                            {
                                name = field.Name,
                                type = reflectionCache.GetName(field.FieldType),
                                value = value
                            }, arrayFields));
                        }
                        else
                        {
                            var childFields = new List<TreeViewItemData<InspectorEntry>>();
                            refreshStack.Push(new StackData
                            {
                                element = value,
                                depth = elem.depth + 1,
                                fields = childFields
                            });
                            elem.fields.Add(new TreeViewItemData<InspectorEntry>(idCounter++, new InspectorEntry
                            {
                                name = field.Name,
                                type = reflectionCache.GetName(field.FieldType),
                                value = value
                            }, childFields));
                        }
                    }
                    else
                    {
                        // no children
                        elem.fields.Add(new TreeViewItemData<InspectorEntry>(idCounter++, new InspectorEntry
                        {
                             name = field.Name,
                             type = reflectionCache.GetName(field.FieldType),
                             value = value
                        }));
                    }

                    UnityEngine.Profiling.Profiler.EndSample();
                }
            }
        }

        private bool ShouldInspectChildren(FieldInfo fieldInfo, object value, int depth)
        {
            var belowDepth = depth < MaxDepth;
            //var isNull = !fieldInfo.FieldType.IsValueType && value == null;
            return belowDepth && 
                value != null &&
                !fieldInfo.FieldType.IsPointer && 
                !fieldInfo.FieldType.IsPrimitive &&
                value is not UnityEngine.Object &&
                ReflectionBlacklist.CanInspect(fieldInfo.FieldType);
        }

        public void Bind(MultiColumnTreeView treeView)
        {
            treeView.SetRootItems(rootItems);
        }

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
        [FSMDebuggerHidden]
        private readonly MultiColumnTreeView treeView;
        [FSMDebuggerHidden]
        private readonly StateHierarchy stateHierarchy = new StateHierarchy();

        public InspectorViewFSMState(DebuggerFSMStateData stateData, VisualElement container)
        {
            this.stateData = stateData;
            this.container = container;
            inspectorEntryPool = new VisualElementPool(AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIMap_InspectorView.InspectorEntryPath));
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIMap_InspectorView.Path);
            inspectorRoot = visualTree.Instantiate();

            var columns = new Columns()
            {
                primaryColumnName = "name",
                reorderable = false,
                resizable = true,
                stretchMode = Columns.StretchMode.GrowAndFill
            };
            columns.Add(new Column
            {
                name = "name",
                title = "Name",
                makeCell = () => new Label(),
                bindCell = (elem, i) => (elem as Label).text = treeView.GetItemDataForIndex<InspectorEntry>(i).name
            });
            columns.Add(new Column
            {
                name = "value",
                title = "Value",
                makeCell = () => new Label(),
                bindCell = (elem, i) => (elem as Label).text = treeView.GetItemDataForIndex<InspectorEntry>(i).value?.ToString() ?? "Null"
            });
            treeView = new MultiColumnTreeView(columns)
            {
                autoExpand = false,
            };
            treeView.SetRootItems(new List<TreeViewItemData<int>> { new TreeViewItemData<int>(1, 1) });
            inspectorRoot.Q(UIMap_InspectorView.StateRoot).Add(treeView);

            //treeView.columns["name"].makeCell = () => new Label();
            //treeView.columns["type"].makeCell = () => new Label();
            //treeView.columns["value"].makeCell = () => new Label();

            //treeView.columns["name"].bindCell = (elem, i) => (elem as Label).text = treeView.GetItemDataForIndex<InspectorEntry>(i).name;
            //treeView.columns["type"].bindCell = (elem, i) => (elem as Label).text = treeView.GetItemDataForIndex<InspectorEntry>(i).type;
            //treeView.columns["value"].bindCell = (elem, i) => (elem as Label).text = treeView.GetItemDataForIndex<InspectorEntry>(i).name;
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
            //var n2 = new TreeViewItemData<InspectorEntry>(1, new InspectorEntry
            //{
            //    name = "n0",
            //    type = "type",
            //    value = null
            //});
            //stateHierarchy.Add(n1);
            //stateHierarchy.Add(n2);
            //treeView.Rebuild();
        }

        public void Exit()
        {
            inspectorRoot.RemoveFromHierarchy();
        }

        public void Update(float delta)
        {
            if (stateData.currentlyInspecting.TryGetActive(out var activeState))
            {
                stateHierarchy.Refresh(activeState);
                stateHierarchy.Bind(treeView);
                treeView.Rebuild();
            }
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