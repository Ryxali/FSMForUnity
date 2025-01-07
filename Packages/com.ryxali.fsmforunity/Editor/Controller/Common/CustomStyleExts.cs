using UnityEngine;
using UnityEngine.UIElements;

namespace FSMForUnity.Editor
{
    internal static class CustomStyleExts
    {
        public static float ValueOrDefault(this ICustomStyle style, StyleWithDefault<float> styleWithDefault)
        {
            return style.TryGetValue(styleWithDefault.property, out var f) ? f : styleWithDefault.defaultValue;
        }

        public static Color ValueOrDefault(this ICustomStyle style, StyleWithDefault<Color> styleWithDefault)
        {
            return style.TryGetValue(styleWithDefault.property, out var f) ? f : styleWithDefault.defaultValue;
        }

        public static int ValueOrDefault(this ICustomStyle style, StyleWithDefault<int> styleWithDefault)
        {
            return style.TryGetValue(styleWithDefault.property, out var f) ? f : styleWithDefault.defaultValue;
        }
    }
}
