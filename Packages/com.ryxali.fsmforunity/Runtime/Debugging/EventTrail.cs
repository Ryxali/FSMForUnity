using System.Collections;
using System.Collections.Generic;

namespace FSMForUnity
{
    internal class EventTrail //: IEnumerable<MachineEvent>
    {
        private readonly MachineEvent[] cyclicalArray;
        private readonly List<MachineEvent> trail;

        private int cyclicalIndexStart;
        private int cyclicalCount;
        private MachineEvent current;
        private IFSMState tail;

        private int tick;

        public EventTrail(int capacity)
        {
            cyclicalArray = new MachineEvent[capacity];
            trail = new List<MachineEvent>();
        }

        public IFSMState GetTail() => tail;

        public void Enqueue(MachineEvent evt)
        {
            evt.tick = tick;
            
            if (evt.type == StateEventType.Update)
                tick++;
            if (evt.type == current.type)
            {
                current.count++;
                trail[trail.Count - 1] = current;
            }
            else
            {
                current = evt;
                trail.Add(current);
            }

            var index = (cyclicalIndexStart + cyclicalCount) % cyclicalArray.Length;
            cyclicalCount++;
            if (cyclicalCount >= cyclicalArray.Length)
            {
                cyclicalCount--;
                tail = cyclicalArray[cyclicalIndexStart].state;
                cyclicalIndexStart = (cyclicalIndexStart + 1) % cyclicalArray.Length;
            }
            cyclicalArray[index] = evt;
        }

        public bool Dequeue(out MachineEvent evt)
        {
            if (cyclicalCount > 0)
            {
                evt = cyclicalArray[cyclicalIndexStart];
                cyclicalIndexStart = (cyclicalIndexStart + 1) % cyclicalArray.Length;
                cyclicalCount--;
                return true;
            }
            else
            {
                evt = default;
                return false;
            }
        }

        public IReadOnlyList<MachineEvent> GetHistory() => trail;

    }
}
