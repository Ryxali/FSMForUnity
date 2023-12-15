using System.Collections.Generic;

namespace FSMForUnity
{
    internal class TriggerDispatch
    {
        public delegate void OnTrigger(uint id);

        public event OnTrigger onTrigger = delegate { };

        private bool isStalled;
        private readonly List<uint> backlog = new List<uint>();

        public void Trigger(uint id)
        {
            if (isStalled)
                backlog.Add(id);
            else
                onTrigger(id);
        }

        public void Stall()
        {
            isStalled = true;
        }

        public void Resume()
        {
            for (int i = 0; i < backlog.Count; i++)
            {
                onTrigger(backlog[i]);
            }
            backlog.Clear();
            isStalled = false;
        }
    }
}
