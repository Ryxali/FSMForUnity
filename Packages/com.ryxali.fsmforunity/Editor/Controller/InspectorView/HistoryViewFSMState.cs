using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FSMForUnity.Editor
{
    internal class HistoryViewFSMState : IFSMState, IMachineEventListener
    {
        private readonly DebuggerFSMStateData stateData;
        private readonly VisualElement container;
        private readonly VisualElement inspectorRoot;

        private readonly MultiColumnListView multiColumnListView;
        private readonly List<Event> items = new List<Event>();
        private readonly InvertedList invertedList;
        private class InvertedList : IList
        {
            private readonly IList list;
            private readonly List<Event> _list;

            public InvertedList(List<Event> list)
            {
                this.list = list;
                _list = list;
            }

            public Event Get(int index) => _list[Index(index)];
            public object this[int index] { get => list[Index(index)]; set => list[Index(index)] = value; }

            public bool IsFixedSize => false;

            public bool IsReadOnly => true;

            public int Count => list.Count;

            public bool IsSynchronized => list.IsSynchronized;

            public object SyncRoot => list.SyncRoot;

            public int Add(object value)
            {
                list.Insert(0, value);
                return list.Count-1;
            }

            public void Clear()
            {
                list.Clear();
            }

            public bool Contains(object value)
            {
                return list.Contains(value);
            }

            public void CopyTo(Array array, int index)
            {
                throw new NotImplementedException();
            }

            public IEnumerator GetEnumerator()
            {
                return new En(list);
            }

            public int Index(int index) => list.Count - 1 - index;

            public int IndexOf(object value)
            {
                return Index(list.IndexOf(value));
            }

            public void Insert(int index, object value)
            {
                list.Insert(Index(index), value);
            }

            public void Remove(object value)
            {
                list.Remove(value);
            }

            public void RemoveAt(int index)
            {
                list.RemoveAt(Index(index));
            }

            private struct En : IEnumerator
            {
                public object Current { get; private set; }

                private readonly IList list;

                public En(IList list) : this()
                {
                    this.list = list;
                }

                private int index;

                public bool MoveNext()
                {
                    index--;
                    if (index >= 0)
                    {
                        Current = list[index];
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                public void Reset()
                {
                    index = list.Count;
                }
            }
        }

        private IFSMState previousState;
        private int updateSourceTick;

        public struct Event
        {
            public string eventName;
            public string tick;
            public string signal;
            public string from;
            public string to;

            public StateEventType type;
        }


        public HistoryViewFSMState(DebuggerFSMStateData stateData, VisualElement container)
        {
            invertedList = new InvertedList(items);
            this.stateData = stateData;
            this.container = container;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIMap_HistoryView.Path);
            inspectorRoot = visualTree.Instantiate();
            multiColumnListView = inspectorRoot.Q<MultiColumnListView>();
            multiColumnListView.itemsSource = invertedList;
            multiColumnListView.columns[0].bindCell = (elem, i) => elem.Q<Label>().text = invertedList.Get(i).eventName;
            multiColumnListView.columns[1].bindCell = (elem, i) => elem.Q<Label>().text = invertedList.Get(i).tick;
            multiColumnListView.columns[2].bindCell = (elem, i) => elem.Q<Label>().text = invertedList.Get(i).signal;
            multiColumnListView.columns[3].bindCell = (elem, i) => elem.Q<Label>().text = invertedList.Get(i).from;
            multiColumnListView.columns[4].bindCell = (elem, i) => elem.Q<Label>().text = invertedList.Get(i).to;
            multiColumnListView.reorderable = false;
            multiColumnListView.canStartDrag += (args) => false;
        }
        public void Enter()
        {
            container.Add(inspectorRoot);
            OnTargetChanged(in stateData.currentlyInspecting);
            stateData.eventBroadcaster.AddListener(this, false);
        }

        public void Exit()
        {
            stateData.eventBroadcaster.RemoveListener(this, false);
            items.Clear();
            multiColumnListView.Clear();
            container.Remove(inspectorRoot);
        }

        public void Update(float delta)
        {
        }

        public void OnTargetChanged(in DebugMachine machine)
        {
            items.Clear();
            previousState = null;
            updateSourceTick = 0;
            multiColumnListView.Clear();
            if (machine.IsValid)
            {
                foreach (var evt in machine.GetHistory())
                {
                    switch (evt.type)
                    {
                        case StateEventType.Enter:
                            if (evt.HasTransition && previousState != null)
                            {
                                items.Add(new Event
                                {
                                    eventName = evt.type.ToString(),
                                    tick = evt.tick.ToString(),
                                    signal = machine.GetTransitionName(evt.transition, previousState, evt.state),
                                    from = machine.GetStateName(previousState),
                                    to = machine.GetStateName(evt.state),
                                    type = evt.type
                                });
                                updateSourceTick = evt.tick;
                            }
                            else
                            {
                                items.Add(new Event
                                {
                                    eventName = evt.type.ToString(),
                                    tick = evt.count.ToString(),
                                    signal = "FSM Enable",
                                    from = "FSM Root",
                                    to = machine.GetStateName(evt.state),
                                    type = evt.type
                                });
                            }
                            previousState = evt.state;
                            updateSourceTick = evt.tick;
                            break;
                        case StateEventType.Update:
                            items.Add(new Event
                            {
                                eventName = evt.type.ToString(),
                                tick = $"{updateSourceTick}-{updateSourceTick+evt.count}",
                                signal = "FSM.Update",
                                from = "N/A",
                                to = machine.GetStateName(evt.state),
                                type = evt.type
                            });
                            break;
                    }
                }
            }
            multiColumnListView.Rebuild();
        }

        public void OnStateEnter(IFSMState state, int tick)
        {
            items.Add(new Event
            {
                eventName = StateEventType.Enter.ToString(),
                tick = tick.ToString(),
                signal = "FSM Enable",
                from = "FSM Root",
                to = stateData.currentlyInspecting.GetStateName(state),
                type = StateEventType.Enter
            }); ;
            previousState = state;
        }

        public void OnStateEnter(IFSMState state, IFSMTransition through, int tick)
        {
            items.Add(new Event
            {
                eventName = StateEventType.Enter.ToString(),
                tick = tick.ToString(),
                signal = stateData.currentlyInspecting.GetTransitionName(through, previousState, state),
                from = stateData.currentlyInspecting.GetStateName(previousState),
                to = stateData.currentlyInspecting.GetStateName(state),
                type = StateEventType.Enter
            });
            previousState = state;
        }

        public void OnStateExit(IFSMState state, int tick)
        {
        }

        public void OnStateExit(IFSMState state, IFSMTransition from, int tick)
        {
        }

        public void OnStateUpdate(IFSMState state, int tick)
        {
            var current = items[items.Count - 1];
            if (current.type == StateEventType.Update)
            {
                current.tick = $"{updateSourceTick}-{tick}";
                items[items.Count - 1] = current;
                multiColumnListView.RefreshItem(0);
            }
            else
            {
                updateSourceTick = tick;
                items.Add(new Event
                {
                    eventName = StateEventType.Update.ToString(),
                    tick = tick.ToString(),
                    signal = "FSM.Update",
                    from = "N/A",
                    to = stateData.currentlyInspecting.GetStateName(state),
                    type = StateEventType.Update
                });
                multiColumnListView.Rebuild();
            }
        }
    }
}
