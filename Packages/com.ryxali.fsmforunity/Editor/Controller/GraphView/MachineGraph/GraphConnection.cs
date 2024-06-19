using UnityEngine;

namespace FSMForUnity.Editor
{
    internal struct GraphConnection
    {
        public IFSMTransition transition;
        public int originIndex;
        public GraphNode origin;
        public int destinationIndex;
        public GraphNode destination;
    }
}
