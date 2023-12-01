using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FSMForUnity
{
	internal class EqualityComparer_AnyTransition : IEqualityComparer<AnyTransition>
    {
        public static readonly EqualityComparer_AnyTransition constant = new EqualityComparer_AnyTransition();
        private EqualityComparer_AnyTransition() { }
        public bool Equals(AnyTransition x, AnyTransition y)
        {
            return System.Object.ReferenceEquals(x.transition, y.transition)
                && System.Object.ReferenceEquals(x.to, y.to);

        }

        public int GetHashCode(AnyTransition obj)
        {
            return System.HashCode.Combine(RuntimeHelpers.GetHashCode(obj.transition), RuntimeHelpers.GetHashCode(obj.to));
        }
    }
}
