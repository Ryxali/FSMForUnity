using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Diagnostics;

namespace FSMForUnity
{
	internal static class DebuggingLinker
    {
        private static readonly Dictionary<Object, FSMMachine> linkedMachines = new Dictionary<Object, FSMMachine>();
        private static readonly List<FSMMachine> allMachines = new List<FSMMachine>();

        private static readonly Dictionary<IFSMState, string> stateNames = new Dictionary<IFSMState, string>(EqualityComparer_IFSMState.constant);

        public static bool TryGetLinkedMachineForObject(Object obj, out FSMMachine machine)
        {
            return linkedMachines.TryGetValue(obj, out machine);
        }

        public static void Unlink(FSMMachine machine)
        {
            foreach (var m in linkedMachines.Where(v => v.Value == machine).ToArray())
            {
                linkedMachines.Remove(m.Key);
            }
            allMachines.Remove(machine);
        }

		public static void Link(FSMMachine machine, Object associatedObject)
		{
            if (associatedObject)
                linkedMachines.Add(associatedObject, machine);
            allMachines.Add(machine);
		}

        public static IReadOnlyList<FSMMachine> GetAllMachines() => allMachines;

		public static void TransmitEvent(FSMMachine machine, object evt)
		{

		}

        /// <summary>
        /// Gives a readable name to this state for use when debugging.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IFSMState WithName(this IFSMState state, string name)
        {
#if DEBUG
            if (stateNames.ContainsKey(state))
                stateNames[state] = name;
            else
                stateNames.Add(state, name);
#endif
            return state;
        }

        public static FSMMachine.IBuilder NamedState(this FSMMachine.IBuilder builder, string name)
        {
            return builder;
        }
    }
}
