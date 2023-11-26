using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using FSMForUnity;
using System.Collections;
using System.Collections.Generic;
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
    }
}
