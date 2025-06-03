#if UNITY_2022_1_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Profiling;
using UnityEngine.UIElements;

namespace FSMForUnity.Editor
{
    internal class StateDataHierarchy
    {
        private const int MaxDepth = 6;

        private static readonly ProfilerMarker bindMarker = new ProfilerMarker("StateDataHierarchy.Bind");
        private static readonly ProfilerMarker refreshMarker = new ProfilerMarker("StateDataHierarchy.Refresh");
        private static readonly ProfilerMarker processMarker = new ProfilerMarker("Process");
        private static readonly ProfilerMarker addElementMarker = new ProfilerMarker("Add Element");

        private static readonly List<TreeViewItemData<InspectorEntry>> emptyChildren = new List<TreeViewItemData<InspectorEntry>>(0);

        [FSMDebuggerHidden]
        private readonly List<TreeViewItemData<InspectorEntry>> rootItems = new List<TreeViewItemData<InspectorEntry>>();
        [FSMDebuggerHidden]
        private readonly ReflectionCache reflectionCache = new ReflectionCache();
        [FSMDebuggerHidden]
        private readonly Stack<StackData> refreshStack = new Stack<StackData>(32);
        [FSMDebuggerHidden]
        private readonly StringFormatCache<int> indexStringCache = new StringFormatCache<int>("[{0}]");
        [FSMDebuggerHidden]
        private readonly BatchResetObjectPool<List<TreeViewItemData<InspectorEntry>>> listPool = new BatchResetObjectPool<List<TreeViewItemData<InspectorEntry>>>(() => new List<TreeViewItemData<InspectorEntry>>(128), l => l.Clear());

        private int idCounter;

        private struct StackData
        {
            public int depth;
            public object element;
            public List<TreeViewItemData<InspectorEntry>> fields;
        }

        public void Refresh(object rootObject)
        {
            refreshMarker.Begin();
            listPool.ReturnAll();
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
                processMarker.Begin();
                var elem = refreshStack.Pop();

                var elemType = elem.element.GetType();
                foreach (var field in reflectionCache.GetFields(elemType))
                {
                    var value = reflectionCache.GetFieldValue(field, elem.element);//field.GetValue(elem.element);

                    if (ShouldInspectChildren(field, value, elem.depth))
                    {
                        if (value is IEnumerable enumerable)
                        {
                            var index = 0;
                            var arrayFields = listPool.Take();
                            for (var en = enumerable.GetEnumerator(); en.MoveNext(); index++)
                            {
                                var indexValue = en.Current;
                                var childFields = listPool.Take();
                                if (indexValue != null)
                                {
                                    refreshStack.Push(new StackData
                                    {
                                        element = indexValue,
                                        depth = elem.depth + 2,
                                        fields = childFields
                                    });
                                }
                                addElementMarker.Begin();
                                arrayFields.Add(new TreeViewItemData<InspectorEntry>(idCounter++, new InspectorEntry
                                {
                                    name = indexStringCache.Get(index),
                                    type = string.Empty,
                                    value = indexValue
                                }, childFields));
                                addElementMarker.End();
                            }
                            addElementMarker.Begin();
                            elem.fields.Add(new TreeViewItemData<InspectorEntry>(idCounter++, new InspectorEntry
                            {
                                name = field.Name,
                                type = reflectionCache.GetTypeName(field.FieldType),
                                value = value
                            }, arrayFields));
                            addElementMarker.End();
                        }
                        else if (value is System.Delegate && value != null)
                        {

                            var childFields = listPool.Take();

                            var childValue = typeof(System.Delegate).GetField("m_target", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(value);
                            if (childValue != null)
                            {
                                refreshStack.Push(new StackData
                                {
                                    element = childValue,
                                    depth = elem.depth + 1,
                                    fields = childFields
                                });
                            }
                            addElementMarker.Begin();
                            elem.fields.Add(new TreeViewItemData<InspectorEntry>(idCounter++, new InspectorEntry
                            {
                                name = field.Name,
                                type = reflectionCache.GetTypeName(field.FieldType),
                                value = value
                            }, childFields));
                            addElementMarker.End();
                        }
                        else
                        {
                            var childFields = listPool.Take();
                            refreshStack.Push(new StackData
                            {
                                element = value,
                                depth = elem.depth + 1,
                                fields = childFields
                            });
                            addElementMarker.Begin();
                            elem.fields.Add(new TreeViewItemData<InspectorEntry>(idCounter++, new InspectorEntry
                            {
                                name = field.Name,
                                type = reflectionCache.GetTypeName(field.FieldType),
                                value = value
                            }, childFields));
                            addElementMarker.End();
                        }
                    }
                    else
                    {
                        addElementMarker.Begin();
                        // no children
                        elem.fields.Add(new TreeViewItemData<InspectorEntry>(idCounter++, new InspectorEntry
                        {
                             name = field.Name,
                             type = reflectionCache.GetTypeName(field.FieldType),
                             value = value
                        }, emptyChildren));
                        addElementMarker.End();
                    }
                }
                processMarker.End();
            }
            refreshMarker.End();
        }

        private bool ShouldInspectChildren(FieldInfo fieldInfo, object value, int depth)
        {
            // TODO expose a list to preferences/settings to allow users to blacklist specific types
            return
                // For performance and to handle cyclical dependencies, always stop at a certain depth 
                depth < MaxDepth && 
                // Never inspect values that don't exist
                value != null &&
                // Accessing pointers can crash the editor
                !fieldInfo.FieldType.IsPointer && 
                // No need to check deeper than the primitive itself (int, string, etc)
                !fieldInfo.FieldType.IsPrimitive &&
                value is not string &&
                // Objects generally contain too much to reveal in a tree view without a performance hit
                value is not UnityEngine.Object && 
                // VisualElements are too data dense to show deeper without a performance hit
                value is not VisualElement;
        }

        public void Bind(MultiColumnTreeView treeView)
        {
            bindMarker.Begin();
            treeView.SetRootItems(rootItems);
            bindMarker.End();
        }

    }
}
#endif