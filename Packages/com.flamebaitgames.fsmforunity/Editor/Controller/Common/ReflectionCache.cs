using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Unity.Profiling;

namespace FSMForUnity.Editor
{
    internal class ReflectionCache
    {
        private static readonly ProfilerMarker getFieldsMarker = new ProfilerMarker("ReflectionCache.GetFields");
        private static readonly ProfilerMarker reflectionMarker = new ProfilerMarker("GetFieldsDeep (Reflection)");
        private static readonly ProfilerMarker getFieldValueMarker = new ProfilerMarker("ReflectionCache.GetFieldValue");
        private static readonly ProfilerMarker emitMarker = new ProfilerMarker("Emit");

        private readonly List<FieldInfo> listBuffer = new List<FieldInfo>(32);
        private readonly Stack<Type> typeStack = new Stack<Type>(32);
        private readonly Dictionary<Type, FieldInfo[]> cachedReflection = new Dictionary<Type, FieldInfo[]>(4096);
        private readonly Dictionary<Type, string> cachedFieldNames = new Dictionary<Type, string>(4096);
        private readonly Dictionary<FieldInfo, GetFieldValueDelegate> fieldGetterDelegates = new Dictionary<FieldInfo, GetFieldValueDelegate>(4096);

        private const BindingFlags GetFieldsFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private delegate object GetFieldValueDelegate(object obj);

        public FieldInfo[] GetFields(System.Type type)
        {
            using (getFieldsMarker.Auto())
            {
                if (cachedReflection.TryGetValue(type, out var fields))
                {
                    return fields;
                }
                else
                {
                    reflectionMarker.Begin();
                    var f = GetFieldsDeep(type);
                    reflectionMarker.End();
                    cachedReflection.Add(type, f);
                    return f;
                }
            }
        }

        public object GetFieldValue(FieldInfo fieldInfo, object obj)
        {
            using (getFieldValueMarker.Auto())
            {
                if (fieldGetterDelegates.TryGetValue(fieldInfo, out var del))
                {
                    return del(obj);
                }
                else
                {
                    emitMarker.Begin();
                    var method = new DynamicMethod($"EmitGetFieldValue_{fieldInfo.Name}", typeof(object), new Type[] { typeof(object) }, fieldInfo.DeclaringType.Module, true);
                    ILGenerator il = method.GetILGenerator(256);
                    il.Emit(OpCodes.Ldarg_0);
                    if (fieldInfo.DeclaringType.IsValueType)
                    {
                        il.Emit(OpCodes.Unbox, fieldInfo.DeclaringType);
                    }
                    il.Emit(OpCodes.Ldfld, fieldInfo);
                    if (fieldInfo.FieldType.IsValueType)
                    {
                        il.Emit(OpCodes.Box, fieldInfo.FieldType);
                    }
                    il.Emit(OpCodes.Ret);

                    var newDelegate = (GetFieldValueDelegate)method.CreateDelegate(typeof(GetFieldValueDelegate));
                    emitMarker.End();
                    fieldGetterDelegates.Add(fieldInfo, newDelegate);
                    return newDelegate(obj);
                }
            }
        }

        public string GetTypeName(Type type)
        {
            if (cachedFieldNames.TryGetValue(type, out var name))
            {
                return name;
            }
            else
            {
                var n = type.Name;
                cachedFieldNames.Add(type, n);
                return n;
            }
        }
        private FieldInfo[] GetFieldsDeep(Type rootType)
        {
            listBuffer.Clear();
            typeStack.Push(rootType);
            while (typeStack.Count > 0)
            {
                var type = typeStack.Pop();
                foreach (var field in type.GetFields(GetFieldsFlags))
                {
                    if (field.GetCustomAttributes(typeof(FSMDebuggerHiddenAttribute), false)?.Length == 0
                        && !field.FieldType.IsPointer)
                    {
                        listBuffer.Add(field);
                    }
                }
                if (type.BaseType != null && ReflectionBlacklist.CanInspect(type.BaseType))
                    typeStack.Push(type.BaseType);
            }
            typeStack.Clear();
            return listBuffer.ToArray();
        }
    }
}
