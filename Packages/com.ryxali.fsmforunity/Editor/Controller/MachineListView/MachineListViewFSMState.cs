using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace FSMForUnity.Editor
{
    /// <summary>
    /// Event based, populates and presents the list view of state machines
    /// the user can inspect.
    /// </summary>
    internal class MachineListViewFSMState : IFSMState
    {
        private readonly VisualElement container;
        private readonly VisualElement listViewRoot;
        private readonly VisualTreeAsset listEntryAsset;
        private readonly List<VisualElement> listElements = new List<VisualElement>(512);
        private readonly ListView listView;

        private readonly List<DebugMachine> listMachines = new List<DebugMachine>();
        private readonly DebuggerFSMStateData stateData;

        public MachineListViewFSMState(DebuggerFSMStateData stateData, VisualElement container)
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
