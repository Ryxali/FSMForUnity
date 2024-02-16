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
            toRect = new Rect(
                to.resolvedStyle.left,
                to.resolvedStyle.top,
                to.resolvedStyle.width,
                to.resolvedStyle.height);

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
            RecalculateLayout();
        }
        private void UpdateGeometryTo(GeometryChangedEvent evt)
        {
            toRect = evt.newRect;
            RecalculateLayout();
        }

        private void RecalculateLayout()
        {
            var from = fromRect;
            var to = toRect;
            var rect = Rect.MinMaxRect(Mathf.Min(from.center.x, to.center.x), Mathf.Min(from.center.y, to.center.y), Mathf.Max(from.center.x, to.center.x), Mathf.Max(from.center.y, to.center.y));
            rect.x -= LineWidth;
            rect.y -= LineWidth;
            rect.width += LineWidth * 2;
            rect.height += LineWidth * 2;
            style.left = new StyleLength(new Length(rect.x));
            style.top = new StyleLength(new Length(rect.y));
            style.width = new StyleLength(new Length(rect.width));
            style.height = new StyleLength(new Length(rect.height));
            fromPoint = from.center - rect.position;
            toPoint = to.center - rect.position;
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
