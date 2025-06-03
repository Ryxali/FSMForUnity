using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FSMForUnity.Editor
{
    internal class HistoryViewFSMState : IFSMState
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
        private int lastSize;

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

            var machine = stateData.currentlyInspecting;
            foreach (var evt in machine.GetHistory())
            {
                AddEvent(in evt);
            }
            lastSize = machine.GetHistory().Count;
        }

        private void AddEvent(in MachineEvent evt)
        {
            var machine = stateData.currentlyInspecting;
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
                    }
                    else
                    {
                        items.Add(new Event
                        {
                            eventName = evt.type.ToString(),
                            tick = evt.tick.ToString(),
                            signal = "FSM Enable",
                            from = "FSM Root",
                            to = machine.GetStateName(evt.state),
                            type = evt.type
                        });
                    }
                    previousState = evt.state;
                    break;
                case StateEventType.Update:
                    items.Add(new Event
                    {
                        eventName = evt.type.ToString(),
                        tick = $"{evt.tick}-{evt.tick + evt.count}",
                        signal = "FSM.Update",
                        from = "N/A",
                        to = machine.GetStateName(evt.state),
                        type = evt.type
                    });
                    break;
            }
        }

        public void Exit()
        {
            items.Clear();
            multiColumnListView.Clear();
            container.Remove(inspectorRoot);
        }

        public void Update(float delta)
        {
            var history = stateData.currentlyInspecting.GetHistory();
            if (items.Count > 0)
            {
                var peek = items[items.Count - 1];
                if (peek.type == StateEventType.Update)
                {
                    var evt = history[lastSize-1];
                    peek.tick = $"{evt.tick}-{evt.tick+evt.count}";
                    items[items.Count - 1] = peek;
                    multiColumnListView.RefreshItem(0);
                }
            }
            if (lastSize != history.Count)
            {
                var t = lastSize;
                for (; lastSize < history.Count; lastSize++)
                {
                    var evt = history[lastSize];
                    AddEvent(in evt);
                }
                multiColumnListView.Rebuild();
                if (multiColumnListView.selectedIndex > 0)
                    multiColumnListView.selectedIndex += lastSize - t;
            }
        }

    }
}
