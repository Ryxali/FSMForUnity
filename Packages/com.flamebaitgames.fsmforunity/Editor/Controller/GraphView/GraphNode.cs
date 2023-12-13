using UnityEngine;

namespace FSMForUnity.Editor.IMGUI
{
    internal struct GraphNode
    {
        public IFSMState state;
        public Vector2 position;
        public bool isDefault;
    }
}
