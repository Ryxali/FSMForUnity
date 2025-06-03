#if DEBUG
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace FSMForUnity
{
    internal sealed class DebuggingFSMMachineBuilder : FSMMachine.IBuilder
    {
        private readonly FSMMachine.IBuilder builder;

        private const BindingFlags GetFieldsFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private Dictionary<IFSMState, string> stateNames = new Dictionary<IFSMState, string>(EqualityComparer_IFSMState.constant);
        private Dictionary<FromToTransition, string> transitionNames = new Dictionary<FromToTransition, string>(EqualityComparer_FromToTransition.constant);
        private Dictionary<AnyTransition, string> anyTransitionNames = new Dictionary<AnyTransition, string>(EqualityComparer_AnyTransition.constant);
        private List<FSMMachine> substates = new List<FSMMachine>();
        private readonly Stack<System.Type> typeStack = new Stack<System.Type>(32);
        private readonly Stack<(object obj, int depth)> valueStack = new Stack<(object, int)>(32);
        private readonly HashSet<object> toSkip = new HashSet<object>();

        private Object debugObject;

        public DebuggingFSMMachineBuilder(FSMMachine.IBuilder builder)
        {
            this.builder = builder;
        }

        public IFSMState AddState(string name, IFSMState state)
        {
            if (string.IsNullOrEmpty(name))
                name = state.GetType().Name;
            stateNames.Add(state, name);
            valueStack.Push((state, 0));
            while (valueStack.Count > 0)
            {
                var stack = valueStack.Pop();
                var obj = stack.obj;
                if (obj is FSMMachine machine)
                {
                    substates.Add(machine);
                }
                else if (obj is IEnumerable en)
                {
                    foreach (var e in en)
                    {
                        if (e != null && toSkip.Add(e))
                            valueStack.Push((e, stack.depth+1));
                    }
                }
                else if(stack.depth < 4)
                {
                    typeStack.Push(obj.GetType());
                    while (typeStack.Count > 0)
                    {
                        var type = typeStack.Pop();
                        foreach (var field in type.GetFields(GetFieldsFlags))
                        {
                            if (!typeof(System.Delegate).IsAssignableFrom(field.FieldType) && !field.FieldType.IsPrimitive)
                            {
                                var fieldObj = field.GetValue(obj);
                                if (fieldObj != null && toSkip.Add(fieldObj))
                                    valueStack.Push((fieldObj, stack.depth+1));
                            }
                        }
                        if (type.BaseType != typeof(object) && type.BaseType != typeof(UnityEngine.Object))
                            typeStack.Push(type.BaseType);
                    }
                }
            }
            toSkip.Clear();
            return builder.AddState(name, state);
        }

        public IFSMTransition AddTransition(string name, IFSMTransition transition, IFSMState from, IFSMState to)
        {
            if (string.IsNullOrEmpty(name))
                name = transition.GetType().Name;
            transitionNames.Add(new FromToTransition { transition = transition, from = from, to = to }, name);
            return builder.AddTransition(name, transition, from, to);
        }

        public IFSMTransition AddAnyTransition(string name, IFSMTransition transition, IFSMState to)
        {
            if (string.IsNullOrEmpty(name))
                name = transition.GetType().Name;
            anyTransitionNames.Add(new AnyTransition { transition = transition, to = to }, name);
            return builder.AddAnyTransition(name, transition, to);
        }

        public void SetDebuggingInfo(string machineName, Object associatedObject)
        {
            debugObject = associatedObject;
            builder.SetDebuggingInfo(machineName, associatedObject);
        }

        public void SetDefaultState(IFSMState state)
        {
            builder.SetDefaultState(state);
        }

        public FSMMachine Complete(FSMMachineFlags behaviourParameters = FSMMachineFlags.Default)
        {
            var trace = new System.Diagnostics.StackTrace(4, true);
            var machine = builder.Complete(behaviourParameters);
            var eventTrail = new EventTrail(FSMConfig.DebugCyclicEventBufferSize);
            machine.eventTransmitter = new MachineEventTransmitter(eventTrail);
            var debugMachine = new DebugMachine(machine, substates, stateNames, transitionNames, anyTransitionNames, eventTrail, trace);
            DebuggingLinker.Link(debugMachine, debugObject);
            substates.Clear();
            stateNames = new Dictionary<IFSMState, string>();
            transitionNames = new Dictionary<FromToTransition, string>();
            anyTransitionNames = new Dictionary<AnyTransition, string>();
            return machine;
        }

        void FSMMachine.IBuilder.Clear()
        {
            debugObject = null;
            builder.Clear();
            stateNames.Clear();
            transitionNames.Clear();
            anyTransitionNames.Clear();
        }
    }
}
#endif
