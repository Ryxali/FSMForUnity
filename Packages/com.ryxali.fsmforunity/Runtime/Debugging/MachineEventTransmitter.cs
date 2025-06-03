using System.Diagnostics;
using UnityEngine;

namespace FSMForUnity
{
    internal struct MachineEventTransmitter
    {
        private readonly EventTrail eventTrail;

        public MachineEventTransmitter(EventTrail eventTrail)
        {
            this.eventTrail = eventTrail;
        }

        [Conditional("UNITY_EDITOR")]
        public void SendStateEvent(StateEventType evt, IFSMState state)
        {
            eventTrail.Enqueue(new MachineEvent
            {
                type = evt,
                state = state,
                count = 1
            });
        }

        [Conditional("UNITY_EDITOR")]
        public void SendTransitionEvent(StateEventType evt, IFSMState state, IFSMTransition through)
        {
            eventTrail.Enqueue(new MachineEvent
            {
                type = evt,
                state = state,
                transition = through,
                count = 1
            });
        }
    }
}
