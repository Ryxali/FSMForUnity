using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using FSMForUnity;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

internal class FSMDebuggerController
{
    private readonly FSMMachine fsm;
    private readonly VisualElement root;

    public FSMDebuggerController(VisualElement root)
    {
        var stateData = new DebuggerFSMStateData();
        var builder = FSMMachine.Build();

        var listView = new ListViewFSMState(stateData, root.Q(UIMap_EditorWindow.ListView));

        var graphBuilder = FSMMachine.Build();
        var graphNoSelected = graphBuilder.AddState(new EmptyFSMState());
        var graphSelected = graphBuilder.AddState(new GraphViewIMGUIFSMState(stateData, root.Q(UIMap_EditorWindow.GraphView)));
        graphBuilder.AddBidirectionalTransition(() => stateData.currentlyInspecting != null, graphNoSelected, graphSelected);
        graphBuilder.SetDebuggingInfo("FSM Debugger Graph", null);

        var inspectorBuilder = FSMMachine.Build();
        var inspectorNoSelected = inspectorBuilder.AddState(new EmptyFSMState());
        var inspectorSelected = inspectorBuilder.AddState(new InspectorViewFSMState(stateData, root.Q(UIMap_EditorWindow.InspectorView)));
        inspectorBuilder.AddBidirectionalTransition(() => stateData.currentlyInspecting != null, inspectorNoSelected, inspectorSelected);
        inspectorBuilder.SetDebuggingInfo("FSM Debugger Inspector", null);

        builder.AddState(new ParallelFSMState(listView, new SubstateFSMState(graphBuilder.Complete()), new SubstateFSMState(inspectorBuilder.Complete())));
        builder.SetDebuggingInfo("FSM Debugger", null);
        fsm = builder.Complete();
        fsm.Enable();
    }

    public void Update(float deltaTime)
    {
        fsm.Update(deltaTime);
    }

    public void Destroy()
    {
        fsm.Destroy();
    }
}

internal static class IMGUIUtil
{
    public static Texture2D GenerateRepeatingGridTexture(int size, int thickness, Color backgroundColor, Color lineColor)
    {
        var tex2D = new Texture2D(size, size);
        for(int y = 0; y < size; y++)
        {
            for(int x = 0; x < size; x++)
            {
                var color = y < thickness || x < thickness ? lineColor : backgroundColor;
                tex2D.SetPixel(x, y, color);
            }
        }
        tex2D.Apply();
        return tex2D;
    }
}

internal struct GraphNode
{
    public IFSMState state;
    public Vector2 position;
    public bool isDefault;
}

internal struct GraphConnection // ??
{
    public Vector2 origin;
    public Vector2 destination;
    public IFSMTransition transition;
}

internal class MachineGraph
{
    private const float TransitionSpringForce = 1f;
    private const float TransitionSpringEqullibrium = 0.75f;
    private const float RepulsionForce = 1f;
    private const float MaxRepulsionForce = 4f;
    private const float StepDelta = 0.15f;
    private const float StepDeltaSqr = StepDelta * StepDelta;
    private const float StepMaxForce = 4f;
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

            var position = Rotate(Vector2.up, (Mathf.PI * 2f/(nodes.Length-1))*(i-1));
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
            me.force = Vector2.zero;
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

internal class GraphViewIMGUIFSMState : IFSMState
{
    private readonly DebuggerFSMStateData stateData;
    private readonly VisualElement container;
    private readonly VisualElement immediateGUIElement;

    private readonly Texture2D gridTexture;
    private readonly Texture2D lineTexture;

    private readonly MachineGraph machineGraph = new MachineGraph();

    private Vector2 panPosition;
    private float zoomLevel;
    private const float DefaultGridTiling = 32f;

    public GraphViewIMGUIFSMState(DebuggerFSMStateData stateData, VisualElement container)
    {
        this.stateData = stateData;
        this.container = container;
        immediateGUIElement = new IMGUIContainer(OnGUI);
        gridTexture = IMGUIUtil.GenerateRepeatingGridTexture(128, 2, new Color(0.2f, 0.2f, 0.2f, 2f), new Color(0.6f, 0.6f, 0.6f, 1f));
        gridTexture.hideFlags = HideFlags.HideAndDontSave;
        lineTexture = new Texture2D(1, 1);
        lineTexture.SetPixel(0,0, Color.white);
        lineTexture.Apply();
        lineTexture.hideFlags = HideFlags.HideAndDontSave;
    }

    public void Enter()
    {
        machineGraph.Regenerate(stateData.currentlyInspecting);
        container.Add(immediateGUIElement);

        // Generate nodes and connections
        // start with default state
        // position other nodes in a radius around default
        // generate transitions
        // use transitions as a spring force
        // try satisfy constraints
        // default state is only fixed node, rest can move
    }

    public void Exit()
    {
        immediateGUIElement.RemoveFromHierarchy();
    }

    public void Update(float delta)
    {
        machineGraph.Regenerate(stateData.currentlyInspecting);

    }

    private void OnGUI()
    {
        var panelRect = new Rect(0, 0, container.resolvedStyle.width, container.resolvedStyle.height);
        GUI.BeginGroup(panelRect);
        var repeatingCoords = new Rect(0, 0, panelRect.width / DefaultGridTiling, panelRect.height / DefaultGridTiling);
        GUI.DrawTextureWithTexCoords(panelRect, gridTexture, repeatingCoords);

        const float BoxSpacing = 300f;

        var stateRect = new Rect(panelRect.width/2, panelRect.height/2, 100, 100);

        foreach(var transition in machineGraph.GetTransitions())
        {
            const float LineWidth = 2f;
            var pointA = stateRect.position + transition.origin * BoxSpacing;
            var pointB = stateRect.position + transition.destination * BoxSpacing;
            var diff = pointB - pointA;
            float a = Mathf.Rad2Deg * Mathf.Atan(diff.y / diff.x);
            if (diff.x < 0)
                a += 180;

            float angle = Vector2.SignedAngle(Vector2.up, pointB -pointA);// Mathf.Atan2 (pointB.y - pointA.y, pointB.x - pointA.x) * 180f / Mathf.PI;
            GUIUtility.RotateAroundPivot(a, pointA);
            GUI.EndClip();
            var rect = new Rect (pointA.x, pointA.y, Vector2.Distance(pointA, pointB), LineWidth);
            GUI.DrawTexture(rect, lineTexture);
            GUIUtility.RotateAroundPivot(-a, pointA);
            GUI.BeginClip(panelRect);
        }

        foreach(var state in machineGraph.GetStates())
        {
            var r = stateRect;
            r.x += state.position.x * BoxSpacing - 50;
            r.y += state.position.y * BoxSpacing - 50;
            if(state.isDefault){

                GUI.Box(r, "(Default) " + state.state.ToString() );
            }else{

                GUI.Box(r, state.state.ToString());
            }
        }

        GUI.EndGroup();
    }

    public void Destroy()
    {
        if(Application.isPlaying)
            Object.Destroy(gridTexture);
        else
            Object.DestroyImmediate(gridTexture);
    }
}
