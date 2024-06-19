using System.Collections.Generic;
using System.Linq;
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
            for (int i = index-2; i >= 0; i -= 2)
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

    internal struct ConnectionCount
    {
        /// <summary>
        /// node index
        /// </summary>
        public int index;
        public int count;
    }
    
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
                stateConnectionCount = stateConnectionCount = new NativeArray<int>((nStates+1) * 8, Allocator.TempJob, NativeArrayOptions.UninitializedMemory)
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

    internal struct CalculateEdgesJob : IJobParallelFor
    {
        private readonly float width;
        private readonly float height;
        [ReadOnly]
        private NativeArray<Vector2> toProcess;
        [WriteOnly]
        private NativeArray<ConnectionEdge> edges;

        private CalculateEdgesJob(NativeArray<Vector2> toProcess, float width, float height, NativeArray<ConnectionEdge> edges)
        {
            this.width = width/2f;
            this.height = height/2f;
            this.toProcess = toProcess;
            this.edges = edges;
        }

        public static JobHandle Solve(GraphConnection[] connections, float width, float height, out NativeArray<ConnectionEdge> edges, JobHandle dependsOn = default)
        {
            var na = new NativeArray<Vector2>(connections.Length*2, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < connections.Length; i++)
            {
                var c = connections[i];
                na[i * 2] = c.origin.position;
                na[i * 2 + 1] = c.destination.position;
            }
            edges = new NativeArray<ConnectionEdge>(na.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var job = new CalculateEdgesJob(na, width, height, edges);

            dependsOn = job.Schedule(edges.Length, 128, dependsOn);
            na.Dispose(dependsOn);
            return dependsOn;
        }

        public void Execute(int index)
        {
            var ix = index;
            var mod = index % 2;
            var origin = toProcess[ix];
            var destination = toProcess[ix + 1 - mod*2];

            var candidate0 = origin + new Vector2(-width, 0) - destination;
            var candidate1 = origin + new Vector2(width, 0) - destination;
            var candidate2 = origin + new Vector2(0, -height) - destination;
            var candidate3 = origin + new Vector2(0, height) - destination;
            var output = ConnectionEdge.Left;

            var dist = candidate0.sqrMagnitude;//Vector2.Distance(candidate0, destination);
            if (candidate1.sqrMagnitude < dist)
            {
                dist = candidate1.sqrMagnitude;
                output = ConnectionEdge.Right;
            }
            if (candidate2.sqrMagnitude < dist)
            {
                dist = candidate2.sqrMagnitude;
                output = ConnectionEdge.Top;
            }
            if (candidate3.sqrMagnitude < dist)
            {
                output = ConnectionEdge.Bottom;
            }

            edges[index] = output;
        }

        private struct Connection
        {
            public Vector2 origin;
            public Vector2 destination;
        }
    }
    internal enum ConnectionEdge
    {
        Left,
        Right,
        Top,
        Bottom
    }
    internal class MachineGraph
    {
        private const float TransitionSpringForce = 1f;
        private const float TransitionSpringEqullibrium = 0.75f;
        private const float RepulsionForce = 1f;
        private const float MaxRepulsionForce = 4f;
        private const float StepDelta = 0.15f;
        private const float StepDeltaSqr = StepDelta * StepDelta;
        private const float StepMaxForce = 100f;
        private const float Drag = 0.95f;
        private const int MaxSimulationCycles = 5000;
        private const float MaxTension = 0.05f;
        private const float MaxTensionSqr = MaxTension * MaxTension;

        private GraphNode[] graphNodes;
        private GraphConnection[] graphConnections;

        public GraphNode[] GetStates()
        {
            return graphNodes;
        }

        public GraphConnection[] GetTransitions()
        {
            return graphConnections;
        }

        public void Regenerate(DebugMachine machine)
        {
            // CreateSimGraphNodes & GraphConnections, simulate until satisfied
            var states = machine.States;

            var defaultState = machine.DefaultState;
            var nodes = new SimGraphNode[states.Length];
            var transitionCount = states.Sum(s => machine.TryGetTransitionsFrom(s, out var t) ? t.Length : 0) + (machine.TryGetAnyTransitions(out var t) ? t.Length : 0);//machine.stateTransitions.Sum(kv => kv.Value.Count()) + machine.anyTransitions.Length * (machine.states.Length-1);
            var transitions = new SimGraphConnection[transitionCount];

            nodes[0] = new SimGraphNode
            {
                state = defaultState,
                position = Vector2.zero,
                previousPosition = Vector2.zero,
                force = Vector2.zero
            };

            var stack = new Stack<(IFSMState state, Vector2 rootPos, int maxCount)>();
            var traversed = new HashSet<IFSMState>(EqualityComparer_IFSMState.constant) { machine.DefaultState };
            if (machine.TryGetTransitionsFrom(defaultState, out var firstTransitions))
            {
                foreach (var firstTransition in firstTransitions)
                    stack.Push((firstTransition.to, Vector2.zero, firstTransitions.Length));
            }

            var nodeI = 1;
            while (stack.Count > 0)
            {
                var s = stack.Pop();
                if (!traversed.Contains(s.state))
                {
                    traversed.Add(s.state);
                    var node = nodes[nodeI];
                    node.force = Vector2.zero;
                    node.state = s.state;
                    var position = s.rootPos + Rotate(Vector2.up, Mathf.PI * 1f / (Mathf.Max(6f, nodes.Length - 1)) * (nodeI - 1));
                    node.position = node.previousPosition = position;
                    nodes[nodeI] = node;
                    nodeI++;
                    if (machine.TryGetTransitionsFrom(s.state, out var ttt))
                    {
                        foreach (var ttts in ttt)
                            stack.Push((ttts.to, node.position, ttt.Length));
                    }
                }
            }
            for (int i = 1; i < nodes.Length; i++)
            {
                var state = states[i];
                var node = nodes[i];
                if (state == defaultState) // move default node to 0 index if found
                {
                    var defaultNode = nodes[0];
                    node.state = defaultNode.state;
                    defaultNode.state = state;
                    nodes[0] = defaultNode;
                }
                if (!traversed.Contains(state))
                {
                    var position = Rotate(Vector2.up, Mathf.PI * 1f / (Mathf.Max(6f, nodes.Length - 1)) * (i - 1));
                    node.position = node.previousPosition = position;
                    node.state = state;
                    node.force = Vector2.zero;
                }
                nodes[i] = node;
            }


            var indexDict = new Dictionary<IFSMState, int>(EqualityComparer_IFSMState.constant);
            for (int i = 0; i < nodes.Length; i++)
                indexDict.Add(nodes[i].state, i);

            var tI = 0;
            foreach (var state in states)
            {
                if (machine.TryGetTransitionsFrom(state, out var mappings))
                {
                    foreach (var transition in mappings)
                    {
                        transitions[tI] = new SimGraphConnection
                        {
                            transition = transition.transition,
                            from = indexDict[state],
                            to = indexDict[transition.to]
                        };
                        tI++;
                    }
                }
                if (machine.TryGetAnyTransitions(out var anyTransitions))
                {
                    foreach (var anyTransition in anyTransitions)
                    {
                        transitions[tI] = new SimGraphConnection
                        {
                            transition = anyTransition.transition,
                            from = indexDict[state],
                            to = indexDict[anyTransition.to]
                        };
                        tI++;
                    }
                }
            }

            StepSimulation(nodes, transitions, -0.05f);
            var tension = 1f;
            for (int i = 0; i < MaxSimulationCycles; i++) // !AreConstraintsSatisfied(nodes);
            {
                tension = StepSimulation(nodes, transitions, i < MaxSimulationCycles/2 ? -0.05f: 0f);
            }

            graphNodes = new GraphNode[nodes.Length];
            for (int i = 0; i < graphNodes.Length; i++)
            {
                var node = nodes[i];
                graphNodes[i] = new GraphNode
                {
                    state = node.state,
                    position = node.position,
                    isDefault = i == 0
                };
            }
            graphConnections = new GraphConnection[transitionCount];
            for (int i = 0; i < transitionCount; i++)
            {
                var transition = transitions[i];
                graphConnections[i] = new GraphConnection
                {
                    transition = transition.transition,
                    originIndex = transition.from,
                    origin = graphNodes[transition.from],
                    destinationIndex = transition.to,
                    destination = graphNodes[transition.to]
                };
            }

        }

        public float MinFloatDistance()
        {
            float min = float.MaxValue;
            for (int i = 0; i < graphNodes.Length; i++)
            {
                var a = graphNodes[i];
                for (int j = i + 1; j < graphNodes.Length; j++)
                {
                    var b = graphNodes[j];
                    var dist = Vector2.Distance(a.position, b.position);
                    if(dist < min)
                        min = dist;
                }
            }
            return min;
        }

        public JobHandle SolveConnectionAnchors(float width, float height, out NativeArray<ConnectionEdge> edges, out NativeArray<ConnectionCount> connections)
        {
            var handle = CalculateEdgesJob.Solve(graphConnections, width, height, out edges);
            handle = CountConnectionsForEdgesJob.Solve(graphConnections, edges, out var stateConnectionCount, handle);
            handle = IndexConnectionsJob.Solve(edges, graphConnections, stateConnectionCount, out connections, handle);
            stateConnectionCount.Dispose(handle);
            return handle;
        }

        private bool AreConstraintsSatisfied(SimGraphNode[] nodes)
        {
            // evaluate tension by selecting the highest force

            var maxTension = 0f;
            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                maxTension = Mathf.Max(node.force.sqrMagnitude, maxTension);
            }
            return maxTension <= MaxTensionSqr;
        }

        private float StepSimulation(SimGraphNode[] nodes, SimGraphConnection[] connections, float gravity)
        {
            // spring the transitions
            // then demagnet the nodes
            var maxTension = 0f;


            for (int i = 0; i < nodes.Length; i++)
            {
                var me = nodes[i];
                for (int otherI = 0; otherI < nodes.Length; otherI++)
                {
                    if (i != otherI)
                    {
                        var other = nodes[otherI];
                        var diff = other.position - me.position;
                        var fromEq = diff.magnitude - TransitionSpringEqullibrium;
                        if (diff.magnitude < TransitionSpringEqullibrium)
                        {
                            var force = diff.normalized * fromEq * RepulsionForce;
                            me.force += force;
                        }
                    }
                }
                nodes[i] = me;
            }

            for (int i = 0; i < connections.Length; i++)
            {
                var connection = connections[i];
                var nodeA = nodes[connection.from];
                var nodeB = nodes[connection.to];

                // F = -kx
                var diff = nodeB.position - nodeA.position;
                var fromEq = diff.magnitude - TransitionSpringEqullibrium;
                var force = diff.normalized * fromEq * TransitionSpringForce;
                nodeA.force += force;
                nodeB.force -= force;
                nodes[connection.from] = nodeA;
                nodes[connection.to] = nodeB;
            }

            for (int i = 1; i < nodes.Length; i++) // start at 1 as index 0 => default state, which is fixed
            {
                var me = nodes[i];
                // verlet integration
                // pos = pos * 2 - prev_pos + acc * dt * dt
                var prev = Vector2.Lerp(me.previousPosition, me.position, Drag);
                me.previousPosition = me.position;
                // me.force += (prev - me.position) * 0.2f;
                me.force += Vector2.down * gravity;
                me.force = me.force.normalized * Mathf.Min(me.force.magnitude, StepMaxForce);
                me.position = me.position * 2f - prev + me.force * StepDeltaSqr;
                maxTension = Mathf.Max(me.force.sqrMagnitude, maxTension);
                me.force = Vector2.zero;
                nodes[i] = me;
            }
            return maxTension;
        }

        /*private static void Simulate(SimGraphNode[] nodesArr, SimGraphConnection[] connectionsArr, int maxSteps)
        {
            var nodes = new NativeArray<SimGraphNode>(nodesArr, Allocator.TempJob);
            var connections = new NativeArray<SimGraphConnection>(connectionsArr, Allocator.TempJob);
            var forces = new NativeArray<Vector2>(nodesArr.Length, Allocator.TempJob);

            for(int i = 0; i < maxSteps; i++)
            {

            }
        }

        private struct NodeForcesJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<SimGraphNode> nodes;
            [WriteOnly]
            public NativeArray<Vector2> forces;

            public void Execute(int index)
            {
                var len = forces.Length;
                var me = nodes[index];
                Vector2 sumForce = me.force;
                for(int i = 0; i < len; i++)
                {
                    if(i != index)
                    {
                        var other = nodes[i];
                        var diff = other.position - me.position;
                        var fromEq = diff.magnitude - TransitionSpringEqullibrium;
                        if(diff.magnitude < TransitionSpringEqullibrium)
                        {
                            var force = diff.normalized * fromEq * RepulsionForce;
                            me.force += force;
                        }
                    }
                }
                forces[index] = sumForce;
            }
        }

        private struct CopyForcesJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<Vector2> forces;
            public NativeArray<SimGraphNode> nodes;

            public void Execute(int index)
            {
                var len = nodes.Length;
                for(int i = 0; i < len; i++)
                {
                    var n = nod
                }
            }
        }

        private struct ConnectionForcesJob : IJobParallelFor
        {
            public NativeArray<SimGraphConnection> connections;
            public NativeArray<Vector2> forces;
            public void Execute(int index)
            {

            }
        }*/

        private struct SimGraphNode
        {
            public IFSMState state;
            public Vector2 position;
            public Vector2 previousPosition;
            public Vector2 force;
        }

        private struct SimGraphConnection
        {
            public IFSMTransition transition;
            public int from;
            public int to;
        }

        private static Vector2 Rotate(Vector2 v, float radians)
        {
            return new Vector2(
                v.x * Mathf.Cos(radians) - v.y * Mathf.Sin(radians),
                v.x * Mathf.Sin(radians) + v.y * Mathf.Cos(radians)
            );
        }

    }
}
