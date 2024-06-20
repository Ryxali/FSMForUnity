using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace FSMForUnity.Editor
{
    internal struct CalculateEdgesJob : IJobParallelFor
    {
        private readonly float width;
        private readonly float height;
        [ReadOnly]
        private NativeArray<Vector2> toProcess;
        [WriteOnly]
        private NativeArray<EdgeTuple> edges;

        private struct EdgeTuple
        {
            public ConnectionEdge from;
            public ConnectionEdge to;
        }

        private CalculateEdgesJob(NativeArray<Vector2> toProcess, float width, float height, NativeArray<ConnectionEdge> edges)
        {
            this.width = width / 2f;
            this.height = height / 2f;
            this.toProcess = toProcess;
            this.edges = edges.Reinterpret<EdgeTuple>(sizeof(ConnectionEdge));
        }

        public static JobHandle Solve(GraphConnection[] connections, float width, float height, out NativeArray<ConnectionEdge> edges, JobHandle dependsOn = default)
        {
            var na = new NativeArray<Vector2>(connections.Length * 2, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < connections.Length; i++)
            {
                var c = connections[i];
                na[i * 2] = c.origin.position;
                na[i * 2 + 1] = c.destination.position;
            }
            edges = new NativeArray<ConnectionEdge>(na.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var job = new CalculateEdgesJob(na, width, height, edges);

            dependsOn = job.Schedule(job.edges.Length, 128, dependsOn);
            na.Dispose(dependsOn);
            return dependsOn;
        }

        public void Execute(int index)
        {
            var origin = toProcess[index * 2];
            var destination = toProcess[index * 2 + 1];

            var origin0 = origin + new Vector2(-width, 0);
            var origin1 = origin + new Vector2(width, 0);
            var origin2 = origin + new Vector2(0, -height);
            var origin3 = origin + new Vector2(0, height);


            var destination0 = destination + new Vector2(-width, 0);
            var destination1 = destination + new Vector2(width, 0);
            var destination2 = destination + new Vector2(0, -height);
            var destination3 = destination + new Vector2(0, height);

            float closest = float.MaxValue;
            var output = (ConnectionEdge.Left, ConnectionEdge.Left);

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    var from = (i) switch
                    {
                        0 => (origin0, ConnectionEdge.Left),
                        1 => (origin1, ConnectionEdge.Right),
                        2 => (origin2, ConnectionEdge.Top),
                        3 => (origin3, ConnectionEdge.Bottom),
                        _ => (origin0, ConnectionEdge.Left)
                    };
                    var to = (j) switch
                    {
                        0 => (destination0, ConnectionEdge.Left),
                        1 => (destination1, ConnectionEdge.Right),
                        2 => (destination2, ConnectionEdge.Top),
                        3 => (destination3, ConnectionEdge.Bottom),
                        _ => (destination0, ConnectionEdge.Left)
                    };
                    var dist = Vector2.Distance(from.Item1, to.Item1);
                    if (dist < closest)
                    {
                        closest = dist;
                        output = (from.Item2, to.Item2);
                    }
                }
            }
            edges[index] = new EdgeTuple { from = output.Item1, to = output.Item2 };
        }

        private struct Connection
        {
            public Vector2 origin;
            public Vector2 destination;
        }
    }
}
