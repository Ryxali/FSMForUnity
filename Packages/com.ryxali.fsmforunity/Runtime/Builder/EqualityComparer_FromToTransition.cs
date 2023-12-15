using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FSMForUnity
{
    internal class EqualityComparer_FromToTransition : IEqualityComparer<FromToTransition>
    {
        public static readonly EqualityComparer_FromToTransition constant = new EqualityComparer_FromToTransition();
        private EqualityComparer_FromToTransition() { }
        public bool Equals(FromToTransition x, FromToTransition y)
        {
            return System.Object.ReferenceEquals(x.transition, y.transition)
                && System.Object.ReferenceEquals(x.from, y.from)
                && System.Object.ReferenceEquals(x.to, y.to);

        }

        public int GetHashCode(FromToTransition obj)
        {
            return System.HashCode.Combine(RuntimeHelpers.GetHashCode(obj.transition), RuntimeHelpers.GetHashCode(obj.from), RuntimeHelpers.GetHashCode(obj.to));
        }
    }
}
