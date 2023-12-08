using UnityEngine;

namespace FSMForUnity.Editor.IMGUIGraph
{
    internal struct GraphConnection
    {
        public Vector2 origin;
        public Vector2 destination;
        public IFSMTransition transition;
    }
}
