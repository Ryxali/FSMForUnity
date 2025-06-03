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
            // TODO build tree from machines and child machines
            // start by finding all machines that aren't a child to anyone, then populate a tree
            var stack = new Stack<MachineNode>();
            var machineList = new List<MachineNode>();
            foreach (var machine in list)
            {
                bool isChild = false;
                foreach (var test in list)
                {
                    isChild |= machine.IsChildOf(in test);
                }
                if (!isChild)
                {
                    stack.Push(new MachineNode { machine = machine });
                    machineList.Add(stack.Peek());
                }
            }
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                foreach (var test in list)
                {
                    if (test.IsChildOf(node.machine))
                    {
                        var n = new MachineNode { machine = test };
                        node.children.Add(n);
                        stack.Push(n);
                    }
                }
            }
            // TODO switch to tree list
            foreach (var root in machineList)
            {
                stack.Push(root);
                while (stack.Count > 0)
                {
                    var n = stack.Pop();
                    listMachines.Add(n.machine);
                    foreach (var m in n.children)
                        stack.Push(m);
                }
            }
            //listMachines.AddRange(list);
            listView.RefreshItems();
        }

        private class MachineNode
        {
            public DebugMachine machine;
            public List<MachineNode> children = new List<MachineNode>();
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
