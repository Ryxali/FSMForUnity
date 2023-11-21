using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace FSMForUnity
{
	internal static class DebuggingLinker
    {
        public static readonly Dictionary<Object, FSMMachine> linkedMachines = new Dictionary<Object, FSMMachine>();
        public static readonly List<FSMMachine> allMachines = new List<FSMMachine>();

        public static void RemoveAllReferences(FSMMachine machine)
        {
            foreach (var m in linkedMachines.Where(v => v.Value == machine).ToArray())
            {
                linkedMachines.Remove(m.Key);
            }
            allMachines.Remove(machine);
        }
    }
}
