using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using FSMForUnity;
using System.Collections;
using System.Reflection;
using System.Linq;

namespace FSMForUnity.Editor.IMGUIGraph
{

	internal static class IMGUIUtil
    {
        public static Texture2D GenerateRepeatingGridTexture(int size, int thickness, Color backgroundColor, Color lineColor)
        {
            var tex2D = new Texture2D(size, size);
            for(int y = 0; y < size; y++)
            {
                for(int x = 0; x < size; x++)
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
            return new Rect(rect.x+amount, rect.y+amount, rect.width-amount*2, rect.height-amount*2);
        }

        public static Texture2D GenerateRepeatingArrowTexture(int width, int height, int thickness, Color color)
        {
            var transparent = new Color();
            var tex2D = new Texture2D(width, height);
            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    var isLine = Mathf.Abs(height/2 - y) <= thickness;
                    var isHead = width-x+Mathf.Abs(height/2 -y)*2 < height;

                    var c = isLine || isHead ? color : transparent;
                    tex2D.SetPixel(x, y, c);
                }
            }
            tex2D.Apply();
            return tex2D;
        }
    }
}
