using UnityEngine;

namespace FSMForUnity.Editor.IMGUI
{

    internal static class IMGUIUtil
    {
        /// <summary>
        /// Generate a square texture which resembles a grid pattern
        /// </summary>
        public static Texture2D GenerateRepeatingGridTexture(int size, int thickness, Color backgroundColor, Color lineColor)
        {
            var tex2D = new Texture2D(size, size);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    var color = y < thickness || x < thickness ? lineColor : backgroundColor;
                    tex2D.SetPixel(x, y, color);
                }
            }
            tex2D.Apply();
            return tex2D;
        }

        public static Rect PadRect(Rect rect, float amount)
        {
            return new Rect(rect.x + amount, rect.y + amount, rect.width - amount * 2, rect.height - amount * 2);
        }


        /// <summary>
        /// Generate a texture of an arrow pointing towards the right.
        /// The head of the arrow is placed in the center of the image.
        /// </summary>
        public static Texture2D GenerateRepeatingArrowTexture(int length, int width, int thickness, Color color)
        {
            var transparent = new Color();
            var tex2D = new Texture2D(length, width);
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    var isLine = Mathf.Abs(width / 2 - y) <= thickness;
                    var isHead = x - length / 2 + width / 2 + Mathf.Abs(width / 2 - y) * 2 < width;
                    var isHeadStart = x > length / 2 - width / 2;

                    var c = isLine || (isHeadStart && isHead) ? color : transparent;
                    tex2D.SetPixel(x, y, c);
                }
            }
            tex2D.Apply();
            return tex2D;
        }

        public static Color Blend(Color src, Color dst)
        {
            return new Color
            (
                dst.r * (1 - src.a) + src.r,
                dst.g * (1 - src.a) + src.g,
                dst.b * (1 - src.a) + src.b,
                dst.a * (1 - src.a) + src.a
            );
        }
    }
}
