#if UNITY_2022_1_OR_NEWER
using System.Collections.Generic;
using Unity.Profiling;
using UnityEditor;
using UnityEngine.UIElements;

namespace FSMForUnity.Editor    
{

    internal class InspectorViewFSMState : IFSMState
    {
        private static readonly ProfilerMarker refreshItemsMarker = new ProfilerMarker("MultiColumnTreeView.RefreshItems");
        private readonly DebuggerFSMStateData stateData;

        private readonly VisualElement container;
        private readonly VisualElement inspectorRoot;
        private readonly MultiColumnTreeView treeView;
        private readonly StateDataHierarchy stateHierarchy = new StateDataHierarchy();
        private readonly ObjectPool<Label> labelPool = new ObjectPool<Label>(() => new Label(), l => l.text = string.Empty);

        private IFSMState currentlyInspecting;

        public InspectorViewFSMState(DebuggerFSMStateData stateData, VisualElement container)
        {
            this.stateData = stateData;
            this.container = container;
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
                makeCell = () => labelPool.Take(),
                destroyCell = (v) => labelPool.Return((Label)v),
                bindCell = (elem, i) => (elem as Label).text = treeView.GetItemDataForIndex<InspectorEntry>(i).name,
                stretchable = true
            });
            columns.Add(new Column
            {
                name = "value",
                title = "Value",
                makeCell = () => labelPool.Take(),
                destroyCell = (v) => labelPool.Return((Label)v),
                bindCell = (elem, i) => (elem as Label).text = treeView.GetItemDataForIndex<InspectorEntry>(i).value?.ToString() ?? "Null",
                stretchable = true,
                
            });
            treeView = new MultiColumnTreeView(columns)
            {
                autoExpand = false,
                selectionType = SelectionType.Single,
            };
            treeView.SetRootItems(new List<TreeViewItemData<int>> { new TreeViewItemData<int>(1, 1) });
            inspectorRoot.Q(UIMap_InspectorView.StateRoot).Add(treeView);
            stateHierarchy.Bind(treeView);
        }

        public void Enter()
        {
            container.Add(inspectorRoot);
        }

        public void Exit()
        {
            inspectorRoot.RemoveFromHierarchy();
        }

        public void Update(float delta)
        {
            var observedState = stateData.selectedState;
            if (observedState == null && stateData.currentlyInspecting.TryGetActive(out var activeState))
            {
                observedState = activeState;
            }
            if (observedState != null)
            {
                stateHierarchy.Refresh(observedState);
                if (currentlyInspecting != observedState)
                {
                    stateHierarchy.Bind(treeView);
                }
                refreshItemsMarker.Begin();
                treeView.RefreshItems();
                refreshItemsMarker.End();
            }
            currentlyInspecting = observedState;
        }

        public void Destroy()
        {

        }
    }
}
#endif