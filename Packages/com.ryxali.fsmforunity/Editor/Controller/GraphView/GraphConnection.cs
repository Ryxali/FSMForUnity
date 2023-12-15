using UnityEngine;

namespace FSMForUnity.Editor.IMGUI
{
    internal struct GraphConnection
    {
        public Vector2 origin;
        public Vector2 destination;
        public IFSMTransition transition;
    }
}
