using System.Collections.Generic;

namespace FSMForUnity
{
    internal class EventBroadcaster
    {
        private DebugMachine target;
        private readonly List<IMachineEventListener> listeners = new List<IMachineEventListener>();

        public void AddListener(IMachineEventListener listener)
        {
            listeners.Add(listener);
        }

        public void RemoveListener(IMachineEventListener listener)
        {
            listeners.Remove(listener);
        }

        public void SetTarget(DebugMachine machine)
        {
            target = machine;
            for(int i = 0; i < listeners.Count; i++)
            {
                listeners[i].OnTargetChanged(in target);
            }
        }

        public void Poll()
        {
            if(target.IsValid)
            {
                while(target.PollEvent(out var evt))
                {
                    switch(evt.type)
                    {
                        case StateEventType.Enter:
                            if(evt.HasTransition)
                            {
                                for(int i = 0; i < listeners.Count; i++)
                                {
                                    listeners[i].OnStateEnter(evt.state);
                                }
                            }
                            else
                            {
                                for(int i = 0; i < listeners.Count; i++)
                                {
                                    listeners[i].OnStateEnter(evt.state, evt.transition);
                                }
                            }
                            break;
                        case StateEventType.Exit:
                            if(evt.HasTransition)
                            {
                                for(int i = 0; i < listeners.Count; i++)
                                {
                                    listeners[i].OnStateExit(evt.state);
                                }
                            }
                            else
                            {
                                for(int i = 0; i < listeners.Count; i++)
                                {
                                    listeners[i].OnStateExit(evt.state, evt.transition);
                                }
                            }
                            break;
                        case StateEventType.Update:
                            for(int i = 0; i < listeners.Count; i++)
                            {
                                listeners[i].OnStateUpdate(evt.state);
                            }
                            break;
                    }
                }
            }
        }
    }
}
