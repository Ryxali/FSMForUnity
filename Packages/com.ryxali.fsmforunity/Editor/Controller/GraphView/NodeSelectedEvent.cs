using UnityEngine.UIElements;

namespace FSMForUnity.Editor
{
    internal class NodeSelectedEvent : EventBase<NodeSelectedEvent>
    {
        public NodeVisualElement element;

        protected override void Init()
        {
            base.Init();
            this.LocalInit();
        }

        private void LocalInit()
        {
            this.bubbles = true;
            this.tricklesDown = false;
        }

        public NodeSelectedEvent() => this.LocalInit();
    }
}
