
namespace FSMForUnity
{
    public struct TriggeredTransition
    {
        private readonly TriggerDispatch dispatch;
        internal readonly uint id;

        internal TriggeredTransition(TriggerDispatch dispatch, uint id)
        {
            this.dispatch = dispatch;
            this.id = id;
        }

        public void Trigger()
        {
            dispatch.Trigger(id);
        }
    }
}
