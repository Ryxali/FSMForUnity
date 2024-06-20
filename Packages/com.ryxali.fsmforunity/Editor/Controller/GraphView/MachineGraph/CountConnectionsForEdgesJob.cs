using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace FSMForUnity.Editor
{
    internal struct CountConnectionsForEdgesJob : IJobParallelFor
    {
        [ReadOnly]
        private NativeArray<ConnectionEdge> edges;
        [ReadOnly]
        private NativeArray<int> connections;
        [WriteOnly] // in, out, LRTB
        private NativeArray<int> stateConnectionCount;

        public static JobHandle Solve(GraphConnection[] connections, NativeArray<ConnectionEdge> edges, out NativeArray<int> stateConnectionCount, JobHandle dependsOn)
        {

            int nStates = 0;
            var conns = new NativeArray<int>(connections.Length * 2, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < conns.Length; i++)
            {
                var c = connections[i / 2];
                conns[i] = i % 2 == 0 ? c.originIndex : c.destinationIndex;
                nStates = Mathf.Max(nStates, Mathf.Max(c.destinationIndex, c.originIndex));
            }

            //Debug.Log(string.Join(", ", conns));
            var job = new CountConnectionsForEdgesJob
            {
                connections = conns,
                edges = edges,
                stateConnectionCount = stateConnectionCount = new NativeArray<int>((nStates + 1) * 8, Allocator.TempJob, NativeArrayOptions.UninitializedMemory)
            };

            dependsOn = job.Schedule(job.stateConnectionCount.Length, 256, dependsOn);
            conns.Dispose(dependsOn);
            return dependsOn;
        }

        public void Execute(int index)
        {
            var edge = (index % 8) switch
            {
                0 => ConnectionEdge.Left,
                1 => ConnectionEdge.Left,
                2 => ConnectionEdge.Right,
                3 => ConnectionEdge.Right,
                4 => ConnectionEdge.Top,
                5 => ConnectionEdge.Top,
                6 => ConnectionEdge.Bottom,
                7 => ConnectionEdge.Bottom,
                _ => default
            };
            var nodeIndex = index / 8;
            var count = 0;
            for (int i = index % 2; i < connections.Length; i += 2)
            {
                if (connections[i] == nodeIndex && edges[i] == edge)
                {
                    count++;
                }
            }
            stateConnectionCount[index] = count;
        }

    }
}
