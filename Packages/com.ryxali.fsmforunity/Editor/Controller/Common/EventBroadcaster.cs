using System.Collections.Generic;
using UnityEngine;

namespace FSMForUnity.Editor
{
    internal class EventBroadcaster
    {
        private DebugMachine target;
        private readonly List<IMachineEventListener> listeners = new List<IMachineEventListener>();

        public void AddListener(IMachineEventListener listener, bool sync = true)
        {
            listeners.Add(listener);
            if (sync && target.TryGetActive(out var state))
            {
                listener.OnStateEnter(state, -1);
            }
        }

        public void RemoveListener(IMachineEventListener listener, bool sync = true)
        {
            listeners.Remove(listener);
            if (sync && target.TryGetActive(out var state))
            {
                listener.OnStateExit(state, -1);
            }
        }

        public void SetTarget(DebugMachine machine)
        {
            target = machine;
            for (int i = 0; i < listeners.Count; i++)
            {
                listeners[i].OnTargetChanged(in target);
            }
        }

        public void Poll()
        {
            if (target.IsValid)
            {
                while (target.PollEvent(out var evt))
                {
                    switch (evt.type)
                    {
                        case StateEventType.Enter:
                            if (evt.HasTransition)
                            {
                                for (int i = 0; i < listeners.Count; i++)
                                {
                                    listeners[i].OnStateEnter(evt.state, evt.transition, evt.tick);
                                }
                            }
                            else
                            {
                                for (int i = 0; i < listeners.Count; i++)
                                {
                                    listeners[i].OnStateEnter(evt.state, evt.tick);
                                }
                            }
                            break;
                        case StateEventType.Exit:
                            if (evt.HasTransition)
                            {
                                for (int i = 0; i < listeners.Count; i++)
                                {
                                    listeners[i].OnStateExit(evt.state, evt.transition, evt.tick);
                                }
                            }
                            else
                            {
                                for (int i = 0; i < listeners.Count; i++)
                                {
                                    listeners[i].OnStateExit(evt.state, evt.tick);
                                }
                            }
                            break;
                        case StateEventType.Update:
                            for (int i = 0; i < listeners.Count; i++)
                            {
                                listeners[i].OnStateUpdate(evt.state, evt.tick);
                            }
                            break;
                    }
                }
            }
        }
    }
}
