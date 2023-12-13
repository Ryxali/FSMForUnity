#if UNITY_2022_1_OR_NEWER
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace FSMForUnity.Editor    
{

    internal class InspectorViewFSMState : IFSMState
    {
        private readonly DebuggerFSMStateData stateData;

        private readonly VisualElement container;
        private readonly VisualElement inspectorRoot;
        private readonly MultiColumnTreeView treeView;
        private readonly StateDataHierarchy stateHierarchy = new StateDataHierarchy();
        private readonly ObjectPool<Label> labelPool = new ObjectPool<Label>(() => new Label(), l => l.text = string.Empty);

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
            if (stateData.currentlyInspecting.TryGetActive(out var activeState))
            {
                stateHierarchy.Refresh(activeState);
                stateHierarchy.Bind(treeView);
                //treeView.Rebuild();
                treeView.RefreshItems();
            }
        }

        public void Destroy()
        {

        }
    }
}
#endif