using UnityEngine;

namespace FSMForUnity.Editor
{
    internal struct GraphNode
    {
        public IFSMState state;
        public Vector2 position;
        public bool isDefault;
    }
}
