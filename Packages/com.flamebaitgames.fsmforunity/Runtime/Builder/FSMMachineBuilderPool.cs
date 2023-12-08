using System.Collections.Generic;
using System.Threading;
using static FSMForUnity.FSMMachine;

namespace FSMForUnity
{
    internal static class FSMMachineBuilderPool
    {
        private static readonly Stack<FSMMachine.IBuilder> builders = new Stack<FSMMachine.IBuilder>();
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
#if DEBUG
                var builder = new SafetyCheckingFSMMachineBuilder(new DebuggingFSMMachineBuilder(new FSMMachineBuilder()));
#else
                var builder = new FSMMachineBuilder();
#endif
                return new PooledFSMMachineBuilder(builder, builders, mut);
            }
        }
    }
}
