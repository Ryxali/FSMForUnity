using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
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
        private readonly TreeView listView;
        private readonly ToolbarMenu optionsMenu;


        private readonly List<TreeViewItemData<DebugMachine>> listMachines = new List<TreeViewItemData<DebugMachine>>();
        private readonly DebuggerFSMStateData stateData;
        private bool showEditorMachines = false;

        public MachineListViewFSMState(DebuggerFSMStateData stateData, VisualElement container)
        {
            this.stateData = stateData;
            this.container = container;
            listEntryAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIMap_ListView.ListEntryPath);
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIMap_ListView.Path);
            listViewRoot = visualTree.Instantiate();

            listView = listViewRoot.Q<TreeView>(UIMap_ListView.Items);
            optionsMenu = listViewRoot.Q<ToolbarMenu>();
            optionsMenu.menu.AppendAction("Show Editor Machines", OnShowEditorMachinesToggle, ShowEditorMachinesToggleStatus);
            listView.SetRootItems(listMachines);
            listView.makeItem = () => listEntryAsset.Instantiate();
            listView.bindItem = (elem, i) => elem.Q<Label>(UIMap_ListView.ListEntryLabel).text = listView.GetItemDataForIndex<DebugMachine>(i).Name;
            listView.selectionType = SelectionType.Single;
        }

        private DropdownMenuAction.Status ShowEditorMachinesToggleStatus(DropdownMenuAction arg)
        {
            return showEditorMachines ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal;
        }

        private void OnShowEditorMachinesToggle(DropdownMenuAction evt)
        {
            showEditorMachines = !showEditorMachines;
            RefreshMachinesInList(DebuggingLinker.GetAllMachines());
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
            listView.itemsChosen += List_onSelectedIndicesChange;
            DebuggingLinker.onAllMachinesChanged += RefreshMachinesInList;
        }

        private void RefreshMachinesInList(IReadOnlyList<DebugMachine> list)
        {
            listMachines.Clear();
            int id = 0;
            var stack = new Stack<(DebugMachine machine, List<TreeViewItemData<DebugMachine>> children)>();
            foreach (var machine in list)
            {
                if (machine.IsEditorMachine && machine.IsEditorMachine != showEditorMachines)
                    continue;
                bool isChild = false;
                foreach (var test in list)
                {
                    isChild |= machine.IsChildOf(in test);
                }
                if (!isChild)
                {
                    stack.Push((machine, new List<TreeViewItemData<DebugMachine>>()));
                    listMachines.Add(new TreeViewItemData<DebugMachine>(id++, machine, stack.Peek().children));
                }
            }
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                foreach (var test in list)
                {
                    if (test.IsChildOf(node.machine))
                    {
                        stack.Push((test, new List<TreeViewItemData<DebugMachine>>()));
                        var n = new TreeViewItemData<DebugMachine>(id++, test, stack.Peek().children);// { machine = test };
                        node.children.Add(n);
                    }
                }
            }
            listView.SetRootItems(listMachines);
            listView.RefreshItems();
        }

        public void Update(float delta)
        {
        }

        public void Exit()
        {
            DebuggingLinker.onAllMachinesChanged -= RefreshMachinesInList;
            listView.itemsChosen -= List_onSelectedIndicesChange;
            listViewRoot.RemoveFromHierarchy();
        }

        public void Destroy()
        {
            listElements.Clear();
        }

        private void OnElementClick(int index)
        {
            if(index >= 0)
                stateData.wantToInspectNext = listView.GetItemDataForIndex<DebugMachine>(index);
        }
    }
}
