using System.Collections.Generic;
using System.Threading;
using static FSMForUnity.FSMMachine;

namespace FSMForUnity
{
	internal static class FSMMachineBuilderPool
    {
        private static readonly Stack<FSMMachineBuilder> builders = new Stack<FSMMachineBuilder>();
        private static Mutex mut = new Mutex();
        public static IBuilder Take()
        {
            if (builders.Count > 0)
            {
                mut.WaitOne();
                var builder = builders.Pop();
                mut.ReleaseMutex();
                return new PooledFSMMachineBuilder(builder, builders, mut);
            }
            else
            {
                return new PooledFSMMachineBuilder(new FSMMachineBuilder(), builders, mut);
            }
        }
    }
}
