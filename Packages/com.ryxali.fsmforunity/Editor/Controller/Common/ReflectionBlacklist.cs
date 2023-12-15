using System.Collections.Generic;
using UnityEngine;

namespace FSMForUnity.Editor
{
    /// <summary>
    /// We blacklist a number of types we don't want to inspect deeper into.
    /// This is in general due to the immense amount of data they store.
    /// </summary>
    internal static class ReflectionBlacklist
    {
        private static readonly HashSet<System.Type> blacklistedTypes = new HashSet<System.Type>
    {
        typeof(string),
        typeof(Mesh),
        typeof(Texture),
        typeof(UnityEngine.UIElements.VisualElement),
        typeof(UnityEngine.UI.Graphic),
        typeof(UnityEngine.UI.Selectable)

    };

        private static readonly Stack<System.Type> typeIter = new Stack<System.Type>();

        public static bool CanInspect(System.Type type)
        {
            typeIter.Push(type);
            while (typeIter.Count > 0)
            {
                var t = typeIter.Pop();
                if (blacklistedTypes.Contains(t))
                {
                    typeIter.Clear();
                    return false;
                }
                else if (t.BaseType != null)
                {
                    typeIter.Push(t.BaseType);
                }
            }
            return true;
        }
    }
}
