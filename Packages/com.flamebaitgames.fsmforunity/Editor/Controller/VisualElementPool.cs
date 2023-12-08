using System.Collections.Generic;
using UnityEngine.UIElements;

namespace FSMForUnity
{
    internal class VisualElementPool
    {
        private readonly Stack<VisualElement> inactiveElements = new Stack<VisualElement>();
        private readonly Stack<VisualElement> activeElements = new Stack<VisualElement>();
        private readonly VisualTreeAsset prefab;

        public VisualElementPool(VisualTreeAsset prefab)
        {
            this.prefab = prefab;
        }

        public VisualElement Take()
        {
            if (inactiveElements.Count > 0)
            {
                var elem = inactiveElements.Pop();
                activeElements.Push(elem);
                return elem;
            }
            else
            {
                var elem = prefab.Instantiate();
                activeElements.Push(elem);
                return elem;
            }
        }

        public void ReturnAll()
        {
            while (activeElements.Count > 0)
            {
                var elem = activeElements.Pop();
                elem.RemoveFromHierarchy();
                inactiveElements.Push(elem);
            }
        }
    }
}
