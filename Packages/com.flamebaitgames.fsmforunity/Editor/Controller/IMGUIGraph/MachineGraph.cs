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
        private const int MaxSimulationCycles = 100;
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

        public void Regenerate(FSMMachine machine)
        {
            // CreateSimGraphNodes & GraphConnections, simulate until satisfied

            var defaultState = machine.defaultState;
            var nodes = new SimGraphNode[machine.states.Length];
            var transitionCount = machine.stateTransitions.Sum(kv => kv.Value.Count()) + machine.anyTransitions.Length * (machine.states.Length-1);
            var transitions = new SimGraphConnection[transitionCount];

            nodes[0] = new SimGraphNode
            {
                state = machine.states[0],
                position = Vector2.zero,
                previousPosition = Vector2.zero,
                force = Vector2.zero
            };
            for(int i = 1; i < nodes.Length; i++)
            {
                var state = machine.states[i];
                var node = nodes[i];
                if(state == defaultState) // move default node to 0 index if found
                {
                    var defaultNode = nodes[0];
                    node.state = defaultNode.state;
                    defaultNode.state = state;
                    nodes[0] = defaultNode;
                }

                var position = Rotate(Vector2.up, Mathf.PI * 2f/(Mathf.Max(6f,nodes.Length-1))*(i-1));
                node.position = node.previousPosition = position;
                node.state = state;
                node.force = Vector2.zero;
                nodes[i] = node;
            }


            var indexDict = new Dictionary<IFSMState, int>(EqualityComparer_IFSMState.constant);
            for(int i = 0; i < nodes.Length; i++)
                indexDict.Add(nodes[i].state, i);

            var tI = 0;
            foreach(var transitionArr in machine.stateTransitions)
            {
                foreach(var transition in transitionArr.Value)
                {
                    transitions[tI] = new SimGraphConnection
                    {
                        transition = transition.transition,
                        from = indexDict[transitionArr.Key],
                        to = indexDict[transition.to]
                    };
                    tI++;
                }
            }
            foreach(var state in machine.states)
            {
                foreach(var anyTransition in machine.anyTransitions)
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

            StepSimulation(nodes, transitions);
            for(int i = 0; i < MaxSimulationCycles && !AreConstraintsSatisfied(nodes); i++)
            {
                StepSimulation(nodes, transitions);
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

        private void StepSimulation(SimGraphNode[] nodes, SimGraphConnection[] connections)
        {
            // spring the transitions
            // then demagnet the nodes

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
                me.force = Vector2.zero;
                nodes[i] = me;
            }
        }

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
