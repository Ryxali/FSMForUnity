using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Diagnostics;

namespace FSMForUnity
{
	internal static class DebuggingLinker
    {
        private static readonly Dictionary<Object, DebugMachine> linkedMachines = new Dictionary<Object, DebugMachine>();
        private static readonly List<DebugMachine> allMachines = new List<DebugMachine>();

        public static bool TryGetLinkedMachineForObject(Object obj, out DebugMachine machine)
        {
            return linkedMachines.TryGetValue(obj, out machine);
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
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void Link(DebugMachine machine, Object associatedObject)
        {
            if (associatedObject)
                linkedMachines.Add(associatedObject, machine);
            allMachines.Add(machine);
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
