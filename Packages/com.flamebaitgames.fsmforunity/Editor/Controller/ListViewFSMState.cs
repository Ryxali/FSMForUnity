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

        public ListViewFSMState(DebuggerFSMStateData stateData, VisualElement container)
        {
            this.stateData = stateData;
            this.container = container;
            listEntryAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIMap_ListView.ListEntryPath);
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIMap_ListView.Path);
            listViewRoot = visualTree.Instantiate();
        }

        public void Enter()
        {
            container.Add(listViewRoot);
        }

        public void Update(float delta)
        {

            listViewRoot.Clear();
            var activeMachines = DebuggingLinker.GetAllMachines();
            for (int i = 0; i < activeMachines.Count; i++)
            {
                if (listElements.Count <= i)
                {
                    var inst = listEntryAsset.Instantiate();
                    listElements.Add(inst);
                    inst.RegisterCallback<MouseDownEvent, int>(OnElementClick, i, TrickleDown.TrickleDown);
                }
                var elem = listElements[i];
                elem.Q<Label>(UIMap_ListView.ListEntryLabel).text = activeMachines[i].Name;
                listViewRoot.Add(elem);
            }
        }

        public void Exit()
        {
            listViewRoot.Clear();
            listViewRoot.RemoveFromHierarchy();
        }

        public void Destroy()
        {
            listElements.Clear();
        }

        private void OnElementClick(MouseDownEvent evt, int index)
        {
            stateData.wantToInspectNext = DebuggingLinker.GetAllMachines()[index];
        }
    }
}
