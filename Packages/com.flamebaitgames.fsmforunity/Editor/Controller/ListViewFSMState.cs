using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace FSMForUnity
{
    internal class ListViewFSMState : IFSMState
    {
        private readonly DebuggerFSMStateData stateData;
        [FSMDebuggerHidden]
        private readonly VisualElement container;
        [FSMDebuggerHidden]
        private readonly VisualElement listViewRoot;
        [FSMDebuggerHidden]
        private readonly VisualTreeAsset listEntryAsset;
        [FSMDebuggerHidden]
        private readonly List<VisualElement> listElements = new List<VisualElement>(512);
        private readonly List<DebugMachine> listMachines = new List<DebugMachine>();
        private readonly ListView listView;
        public ListViewFSMState(DebuggerFSMStateData stateData, VisualElement container)
        {
            this.stateData = stateData;
            this.container = container;
            listEntryAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIMap_ListView.ListEntryPath);
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIMap_ListView.Path);
            listViewRoot = visualTree.Instantiate();

            listView = listViewRoot.Q<ListView>(UIMap_ListView.Items);
            listView.itemsSource = listMachines;
            listView.makeItem = () => listEntryAsset.Instantiate();
            listView.bindItem = (elem, i) => elem.Q<Label>(UIMap_ListView.ListEntryLabel).text = listMachines[i].Name;
            listView.selectionType = SelectionType.Single;
        }

        private void List_onSelectedIndicesChange(IEnumerable<object> selected)
        {
            OnElementClick(listView.selectedIndex);
        }

        public void Enter()
        {
            RefreshMachinesInList(DebuggingLinker.GetAllMachines());
            container.Add(listViewRoot);
            OnElementClick(listView.selectedIndex);
            listView.onItemsChosen += List_onSelectedIndicesChange;
            DebuggingLinker.onAllMachinesChanged += RefreshMachinesInList;
        }

        private void RefreshMachinesInList(IReadOnlyList<DebugMachine> list)
        {
            listMachines.Clear();
            listMachines.AddRange(list);
            listView.RefreshItems();
        }

        public void Update(float delta)
        {
            //listViewRoot.Clear();
            //var activeMachines = DebuggingLinker.GetAllMachines();
            //for (int i = 0; i < activeMachines.Count; i++)
            //{
            //    if (listElements.Count <= i)
            //    {
            //        var inst = listEntryAsset.Instantiate();
            //        listElements.Add(inst);
            //        inst.RegisterCallback<MouseDownEvent, int>(OnElementClick, i, TrickleDown.TrickleDown);
            //    }
            //    var elem = listElements[i];
            //    elem.Q<Label>(UIMap_ListView.ListEntryLabel).text = activeMachines[i].Name;
            //    listViewRoot.Add(elem);
            //}
        }

        public void Exit()
        {
            DebuggingLinker.onAllMachinesChanged -= RefreshMachinesInList;
            listView.onItemsChosen -= List_onSelectedIndicesChange;
            listViewRoot.RemoveFromHierarchy();
        }

        public void Destroy()
        {
            listElements.Clear();
        }

        private void OnElementClick(int index)
        {
            if(0 <= index && index < listMachines.Count)
                stateData.wantToInspectNext = listMachines[index];
        }
    }
}
