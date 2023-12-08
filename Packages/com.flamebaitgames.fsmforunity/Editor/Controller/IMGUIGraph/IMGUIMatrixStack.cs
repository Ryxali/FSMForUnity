using System.Collections.Generic;
using UnityEngine;

namespace FSMForUnity.Editor.IMGUIGraph
{
    /// <summary>
    /// Used to make scoped GUI.matrix pushes and pops
    /// </summary>
	internal static class IMGUIMatrixStack
    {
        private static readonly Stack<GUIMatrix> stack = new Stack<GUIMatrix>();
        private static readonly Stack<GUIMatrix> pool = new Stack<GUIMatrix>();

        /// <summary>
        /// Begin a scoped matrix push to GUI.matrix.
        /// put it in a using statement and it will automatically pop once scope is left
        /// <code>
        /// using (IMGUIMatrixStack.Auto(GUI.matrix * myMatrix))
        /// {
        ///     GUI.Box(localRect, content);
        /// }
        /// </code>
        /// </summmary>
        public static System.IDisposable Auto(Matrix4x4 matrix)
        {
            GUIMatrix guiMatrix;
            if (pool.Count > 0)
            {
                guiMatrix = pool.Pop();
            }
            else
            {
                guiMatrix = new GUIMatrix(stack, pool);
            }
            stack.Push(guiMatrix);
            guiMatrix.Begin(matrix);
            return guiMatrix;
        }

        private class GUIMatrix : System.IDisposable
        {
            private readonly Stack<GUIMatrix> stack;
            private readonly Stack<GUIMatrix> pool;
            private Matrix4x4 previous;

            public GUIMatrix(Stack<GUIMatrix> stack, Stack<GUIMatrix> pool)
            {
                this.stack = stack;
                this.pool = pool;
            }


            public void Begin(Matrix4x4 matrix)
            {
                previous = GUI.matrix;
                GUI.matrix = matrix;
            }

            public void Dispose()
            {
                stack.Pop();
                pool.Push(this);
                GUI.matrix = previous;
            }
        }
    }
}
