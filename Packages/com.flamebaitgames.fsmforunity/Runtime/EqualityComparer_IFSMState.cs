using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace FSMForUnity
{
    /// <summary>
    /// This is the fixed comparison method used by the <see cref="FSMMachine"/> when
    /// querying its hashtables (Dictionaries). The machine relies on ReferenceEquals
    /// for mapping transitions between states, and by adding this we ensure that
    /// this behaviour isn't undercut by any state implementing its own GetHashCode
    /// and Equals method.
    /// </summary>
    internal sealed class EqualityComparer_IFSMState : IEqualityComparer<IFSMState>
    {
        public static readonly EqualityComparer_IFSMState constant = new EqualityComparer_IFSMState();

        private EqualityComparer_IFSMState() {}

        public int GetHashCode(IFSMState state)
        {
            return RuntimeHelpers.GetHashCode(state);
        }

        public bool Equals(IFSMState one, IFSMState another)
        {
            return Object.ReferenceEquals(one, another);
        }
    }
}
