using FSMForUnity;
using UnityEngine;

public class TestGraph
{
    private static FSMMachine fsm;
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
    private static void Test()
    {
        var builder = FSMMachine.Build();
        var a = builder.AddLambdaState("A");
        var b = builder.AddLambdaState("B");
        var c = builder.AddLambdaState("C");
        var d = builder.AddLambdaState("D");
        builder.AddLambdaTransition(() => true, a, b);
        builder.AddLambdaTransition(() => false, b, c);
        builder.AddLambdaTransition(() => false, c, a);
        builder.AddLambdaTransition(() => false, c, d);
        builder.SetDebuggingInfo("TestMachine", null);
        fsm = builder.Complete();
        fsm.Enable();
        fsm.Update(0f);
    }
#endif
}
