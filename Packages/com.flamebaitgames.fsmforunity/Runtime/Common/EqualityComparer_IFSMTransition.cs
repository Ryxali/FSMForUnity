using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace FSMForUnity
{
	internal sealed class EqualityComparer_IFSMTransition : IEqualityComparer<IFSMTransition>
    {
        public static readonly EqualityComparer_IFSMTransition constant = new EqualityComparer_IFSMTransition();

        private EqualityComparer_IFSMTransition() { }

        public int GetHashCode(IFSMTransition state)
        {
            return RuntimeHelpers.GetHashCode(state);
        }

        public bool Equals(IFSMTransition one, IFSMTransition another)
        {
            return Object.ReferenceEquals(one, another);
        }
    }
}
