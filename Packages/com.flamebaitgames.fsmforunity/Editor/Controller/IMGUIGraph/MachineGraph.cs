using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using FSMForUnity;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace FSMForUnity.Editor.IMGUIGraph
{
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
        private const int MaxSimulationCycles = 1000;
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
                state = states[0],
                position = Vector2.zero,
                previousPosition = Vector2.zero,
                force = Vector2.zero
            };
            for(int i = 1; i < nodes.Length; i++)
            {
                var state = states[i];
                var node = nodes[i];
                if(state == defaultState) // move default node to 0 index if found
                {
                    var defaultNode = nodes[0];
                    node.state = defaultNode.state;
                    defaultNode.state = state;
                    nodes[0] = defaultNode;
                }

                var position = Rotate(Vector2.up,  Mathf.PI * 1f/(Mathf.Max(6f,nodes.Length-1))*(i-1));
                node.position = node.previousPosition = position;
                node.state = state;
                node.force = Vector2.zero;
                nodes[i] = node;
            }


            var indexDict = new Dictionary<IFSMState, int>(EqualityComparer_IFSMState.constant);
            for(int i = 0; i < nodes.Length; i++)
                indexDict.Add(nodes[i].state, i);

            var tI = 0;
            foreach(var state in states)
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

            StepSimulation(nodes, transitions);
            var tension = 1f;
            for(int i = 0; i < MaxSimulationCycles && tension > MaxTensionSqr; i++) // !AreConstraintsSatisfied(nodes);
            {
                tension = StepSimulation(nodes, transitions);
            }

            graphNodes = new GraphNode[nodes.Length];
            for(int i = 0; i < graphNodes.Length; i++)
            {
                var node = nodes[i];
                graphNodes[i] = new GraphNode{
                    state = node.state,
                    position = node.position,
                    isDefault = i == 0
                };
            }
            graphConnections = new GraphConnection[transitionCount];
            for(int i = 0; i < transitionCount; i++)
            {
                var transition = transitions[i];
                graphConnections[i] = new GraphConnection
                {
                    transition = transition.transition,
                    origin = nodes[transition.from].position,
                    destination = nodes[transition.to].position
                };
            }

        }

        private bool AreConstraintsSatisfied(SimGraphNode[] nodes)
        {
            // evaluate tension by selecting the highest force

            var maxTension = 0f;
            for(int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                maxTension = Mathf.Max(node.force.sqrMagnitude, maxTension);
            }
            return maxTension <= MaxTensionSqr;
        }

        private float StepSimulation(SimGraphNode[] nodes, SimGraphConnection[] connections)
        {
            // spring the transitions
            // then demagnet the nodes
            var maxTension = 0f;

            for(int i = 0; i < nodes.Length; i++)
            {
                var me = nodes[i];
                for(int otherI = 0; otherI < nodes.Length; otherI++)
                {
                    if(i != otherI)
                    {
                        var other = nodes[otherI];
                        var diff = other.position - me.position;
                        var fromEq = diff.magnitude - TransitionSpringEqullibrium;
                        if(diff.magnitude < TransitionSpringEqullibrium)
                        {
                            var force = diff.normalized * fromEq * RepulsionForce;
                            me.force += force;
                        }
                    }
                }
                nodes[i] = me;
            }

            for(int i = 0; i < connections.Length; i++)
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

            for(int i = 1; i < nodes.Length; i++) // start at 1 as index 0 => default state, which is fixed
            {
                var me = nodes[i];
                // verlet integration
                // pos = pos * 2 - prev_pos + acc * dt * dt
                var prev = Vector2.Lerp(me.previousPosition, me.position, Drag);
                me.previousPosition = me.position;
                // me.force += (prev - me.position) * 0.2f;
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

        private static Vector2 Rotate(Vector2 v, float radians) {
            return new Vector2(
                v.x * Mathf.Cos(radians) - v.y * Mathf.Sin(radians),
                v.x * Mathf.Sin(radians) + v.y * Mathf.Cos(radians)
            );
        }

    }
}
