using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace FSMForUnity.Editor
{
    internal struct IndexConnectionsJob : IJobParallelFor
    {
        [ReadOnly]
        private NativeArray<ConnectionEdge> edges;
        [ReadOnly]
        private NativeArray<int> connections;
        [ReadOnly] // in, out, LRTB
        private NativeArray<int> stateConnectionCount;
        [WriteOnly]
        private NativeArray<ConnectionCount> connectionIndices;

        public static JobHandle Solve(NativeArray<ConnectionEdge> edges, GraphConnection[] connections, NativeArray<int> stateConnectionCount, out NativeArray<ConnectionCount> connectionIndices, JobHandle dependsOn = default)
        {
            int nStates = 0;
            var conns = new NativeArray<int>(connections.Length * 2, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < conns.Length; i++)
            {
                var c = connections[i / 2];
                conns[i] = i % 2 == 0 ? c.originIndex : c.destinationIndex;
                nStates = Mathf.Max(nStates, Mathf.Max(c.destinationIndex, c.originIndex));
            }

            var job = new IndexConnectionsJob
            {
                edges = edges,
                connections = conns,
                stateConnectionCount = stateConnectionCount,
                connectionIndices = connectionIndices = new NativeArray<ConnectionCount>(edges.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory)
            };

            dependsOn = job.Schedule(job.connectionIndices.Length, 128, dependsOn);
            conns.Dispose(dependsOn);
            return dependsOn;
        }

        public void Execute(int index)
        {
            var connection = connections[index];
            var edge = edges[index];
            var stateOffset = edge switch
            {
                ConnectionEdge.Left => 0,
                ConnectionEdge.Right => 2,
                ConnectionEdge.Top => 4,
                ConnectionEdge.Bottom => 6,
                _ => default
            } + index % 2;

            var count = stateConnectionCount[connection * 8 + stateOffset];
            var incr = 0;
            for (int i = index - 2; i >= 0; i -= 2)
            {
                if (connections[i] == connection && edges[i] == edge)
                {
                    incr++;
                }
            }
            connectionIndices[index] = new ConnectionCount
            {
                index = incr,
                count = count
            };
        }
    }
}
