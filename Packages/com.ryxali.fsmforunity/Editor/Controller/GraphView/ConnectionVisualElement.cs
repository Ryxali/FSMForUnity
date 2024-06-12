using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace FSMForUnity.Editor
{
    internal class ConnectionVisualElement : VisualElement
    {

        private float LineWidth = 5f;

        private VisualElement from, to;

        private Rect fromRect, toRect;

        private Vector2 fromPoint, toPoint;

        public ConnectionVisualElement()
        {
            style.position = new StyleEnum<Position>(Position.Absolute);
            generateVisualContent = Generate;
        }

        public void Connect(VisualElement from, VisualElement to)
        {
            if (this.from != null)
                this.from.UnregisterCallback<GeometryChangedEvent>(UpdateGeometryFrom);
            this.from = from;
            this.from.RegisterCallback<GeometryChangedEvent>(UpdateGeometryFrom);
            if(this.to != null)
                this.to.UnregisterCallback<GeometryChangedEvent>(UpdateGeometryTo);
            this.to = to;
            this.to.RegisterCallback<GeometryChangedEvent>(UpdateGeometryTo);
            fromRect = new Rect(
                from.resolvedStyle.left,
                from.resolvedStyle.top,
                from.resolvedStyle.width,
                from.resolvedStyle.height);
            fromRect.x -= fromRect.width / 2f;
            fromRect.y -= fromRect.height / 2f;
            toRect = new Rect(
                to.resolvedStyle.left,
                to.resolvedStyle.top,
                to.resolvedStyle.width,
                to.resolvedStyle.height);
            toRect.x -= toRect.width / 2f;
            toRect.y -= toRect.height / 2f;

            RecalculateLayout();
        }

        public void Reset()
        {
            if (from != null)
                from.UnregisterCallback<GeometryChangedEvent>(UpdateGeometryFrom);
            if (to != null)
                to.UnregisterCallback<GeometryChangedEvent>(UpdateGeometryTo);
            from = to = null;
            fromRect = toRect = default;
            fromPoint = toPoint = default;
        }

        private void UpdateGeometryFrom(GeometryChangedEvent evt)
        {
            fromRect = evt.newRect;
            fromRect.x -= evt.newRect.width / 2f;
            fromRect.y -= evt.newRect.height / 2f;
            RecalculateLayout();
        }
        private void UpdateGeometryTo(GeometryChangedEvent evt)
        {
            toRect = evt.newRect;
            toRect.x -= evt.newRect.width / 2f;
            toRect.y -= evt.newRect.height / 2f;
            RecalculateLayout();
        }

        private void RecalculateLayout()
        {
            var from = Closest(fromRect, toRect);
            var to = Closest(toRect, fromRect);
            //var fromX = Mathf.Min(from.xMin - to.center.x, from.xMax - to.center.x) + to.center.x;
            //var fromY = Mathf.Min(from.yMin - to.center.y, from.yMax - to.center.y) + to.center.y;
            //var toX = Mathf.Min(to.xMin - from.center.x, to.xMax - from.center.x) + from.center.x;
            //var toY = Mathf.Min(to.yMin - from.center.y, to.yMax - from.center.y) + from.center.y;
            var rect = Rect.MinMaxRect(Mathf.Min(from.x, to.x), Mathf.Min(from.y, to.y), Mathf.Max(from.x, to.x), Mathf.Max(from.y, to.y));
            rect.x -= LineWidth;
            rect.y -= LineWidth;
            rect.width += LineWidth * 2;
            rect.height += LineWidth * 2;
            style.left = new StyleLength(new Length(rect.x));
            style.top = new StyleLength(new Length(rect.y));
            style.width = new StyleLength(new Length(rect.width));
            style.height = new StyleLength(new Length(rect.height));
            fromPoint = from - rect.position;//from.center - rect.position;
            toPoint = to - rect.position;// to.center - rect.position;
        }

        private static Vector2 Closest(Rect from, Rect to)
        {
            var candidate0 = new Vector2(from.xMax, from.center.y);
            var candidate1 = new Vector2(from.xMin, from.center.y);
            var candidate2 = new Vector2(from.center.x, from.yMin);
            var candidate3 = new Vector2(from.center.x, from.yMax);
            var candidate = candidate0;
            var dist = Vector2.Distance(candidate0, to.center);
            float distBuf;
            if ((distBuf = Vector2.Distance(candidate1, to.center)) < dist)
            {
                dist = distBuf;
                candidate = candidate1;
            }
            if ((distBuf = Vector2.Distance(candidate2, to.center)) < dist)
            {
                dist = distBuf;
                candidate = candidate2;
            }
            if ((distBuf = Vector2.Distance(candidate3, to.center)) < dist)
            {
                candidate = candidate3;
            }
            return candidate;
        }

        private static Rect MinMax(Rect from, Rect to)
        {
            var fromX = Mathf.Min(from.xMin - to.center.x, from.xMax - to.center.x) + to.center.x;
            var fromY = Mathf.Min(from.yMin - to.center.y, from.yMax - to.center.y) + to.center.y;
            var toX = Mathf.Min(to.xMin - from.center.x, to.xMax - from.center.x) + from.center.x;
            var toY = Mathf.Min(to.yMin - from.center.y, to.yMax - from.center.y) + from.center.y;

            return Rect.MinMaxRect(Mathf.Min(fromX, toX), Mathf.Min(fromY, toY), Mathf.Max(fromX, toX), Mathf.Max(fromY, toY));
        }

        private static float Min(float a, float b, float c, float d)
        {
            return Mathf.Min(
                Mathf.Min(a, b),
                Mathf.Min(c, d)
                );
        }

        private static float Max(float a, float b, float c, float d)
        {
            return Mathf.Max(
                Mathf.Max(a, b),
                Mathf.Max(c, d)
                );
        }

        private void Generate(MeshGenerationContext context)
        {
            var from = fromPoint;
            var to = toPoint;
            var painter = context.painter2D;
            painter.BeginPath();
            painter.MoveTo(from);
            painter.LineTo(to);

            painter.lineCap = LineCap.Round;
            painter.lineWidth = LineWidth;
            painter.strokeColor = Color.white;
            painter.Stroke();
        }
    }
}
