using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FSMForUnity
{
    internal static class DebuggingLinker
    {
        private static readonly Dictionary<Object, DebugMachine> linkedMachines = new Dictionary<Object, DebugMachine>();
        private static readonly List<DebugMachine> allMachines = new List<DebugMachine>();
        private static readonly List<Component> getComponents = new List<Component>();
        public static event System.Action<IReadOnlyList<DebugMachine>> onAllMachinesChanged = delegate { };

        public static bool TryGetLinkedMachineForObject(Object obj, out DebugMachine machine)
        {
            if (obj is GameObject go)
            {
                if (linkedMachines.TryGetValue(go, out machine))
                {
                    return true;
                }
                else
                {
                    go.GetComponents(getComponents);
                    foreach (var comp in getComponents)
                    {
                        if (linkedMachines.TryGetValue(comp, out machine))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            else
            {
                return linkedMachines.TryGetValue(obj, out machine);
            }
        }
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Unlink(IDebuggableMachine machine)
        {
            var toRemove = new DebugMachine(machine);
            allMachines.Remove(toRemove);
            foreach (var m in linkedMachines.Where(v => v.Value == toRemove).ToArray())
            {
                linkedMachines.Remove(m.Key);
            }
            onAllMachinesChanged(allMachines);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Link(DebugMachine machine, Object associatedObject)
        {
            if (associatedObject)
                linkedMachines.Add(associatedObject, machine);
            allMachines.Add(machine);
            onAllMachinesChanged(allMachines);
        }

        public static IReadOnlyList<DebugMachine> GetAllMachines() => allMachines;


        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void TransmitEvent(this IDebuggableMachine machine, StateEventType evt, IFSMState state)
        {
            //UnityEngine.Debug.Log($"{machine.GetName()} {evt} {state}");
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void TransmitEvent(this IDebuggableMachine machine, StateEventType evt, IFSMState state, IFSMTransition through)
        {
            //UnityEngine.Debug.Log($"{machine.GetName()} {evt} {state} Through {through}");
        }

        public static FSMMachine.IBuilder NamedState(this FSMMachine.IBuilder builder, string name)
        {
            return builder;
        }
    }
}
