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

        private Vector2 fromPoint, toPoint, control0, control1, fromDir, toDir;
        private ConnectionEdge fromEdge, toEdge;
        private float fromDelta, toDelta;

        public ConnectionVisualElement()
        {
            style.position = new StyleEnum<Position>(Position.Absolute);
            generateVisualContent = Generate;
        }

        public void Connect(VisualElement from, ConnectionEdge fromEdge, float fromDelta, VisualElement to, ConnectionEdge toEdge, float toDelta)
        {
            Debug.Log($"{fromEdge}:{fromDelta:F2} => {toEdge}:{toDelta:F2}");
            this.fromEdge = fromEdge;
            this.toEdge = toEdge;
            this.fromDelta = fromDelta;
            this.toDelta = toDelta;
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
            var from = Offset(fromRect, fromEdge, Mathf.Lerp(0.25f, 1f, fromDelta));//Closest(fromRect, toRect, out fromDir);
            var to = Offset(toRect, toEdge, -Mathf.Lerp(0.25f, 1f, toDelta));//Closest(toRect, fromRect, out toDir);
            fromDir = Dir(fromEdge);
            toDir = Dir(toEdge);
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
            var len = Vector2.Distance(from, to) * 0.35f;
            control0 = from + fromDir * len - rect.position;
            control1 = to + toDir * len - rect.position;
        }

        private static Vector2 Dir(ConnectionEdge edge)
        {
            return edge switch
            {
                ConnectionEdge.Bottom => Vector2.up,
                ConnectionEdge.Top => Vector2.down,
                ConnectionEdge.Left => Vector2.left,
                ConnectionEdge.Right => Vector2.right,
                _ => default
            };
        }

        private static Vector2 InvDir(ConnectionEdge edge)
        {
            return edge switch
            {
                ConnectionEdge.Bottom => Vector2.right,
                ConnectionEdge.Top => Vector2.left,
                ConnectionEdge.Left => Vector2.up,
                ConnectionEdge.Right => Vector2.down,
                _ => default
            };
        }

        private static Vector2 Offset(Rect rect, ConnectionEdge edge, float delta)
        {
            Debug.Log($"{edge}, {delta:F2}");
            var output =  edge switch
            {
                ConnectionEdge.Bottom => new Vector2(rect.center.x, rect.yMax),
                ConnectionEdge.Top => new Vector2(rect.center.x, rect.yMin),
                ConnectionEdge.Left => new Vector2(rect.xMin, rect.center.y),
                ConnectionEdge.Right => new Vector2(rect.xMax, rect.center.y),
                _ => rect.center
            };
            var dir = InvDir(edge);
            //dir = new Vector2(dir.y, -dir.x);

            var size = Mathf.Abs(Vector2.Dot(dir, rect.size*0.5f));
            output += dir * size * delta; 
            return output;
        }

        private static Vector2 Closest(Rect from, Rect to, out Vector2 direction)
        {
            var candidate0 = new Vector2(from.xMax, from.center.y);
            var candidate1 = new Vector2(from.xMin, from.center.y);
            var candidate2 = new Vector2(from.center.x, from.yMin);
            var candidate3 = new Vector2(from.center.x, from.yMax);
            var candidate = candidate0;
            direction = Vector2.right;

            var dist = Vector2.Distance(candidate0, to.center);
            float distBuf;
            if ((distBuf = Vector2.Distance(candidate1, to.center)) < dist)
            {
                dist = distBuf;
                candidate = candidate1;
                direction = Vector2.left;
            }
            if ((distBuf = Vector2.Distance(candidate2, to.center)) < dist)
            {
                dist = distBuf;
                candidate = candidate2;
                direction = Vector2.down;
            }
            if (Vector2.Distance(candidate3, to.center) < dist)
            {
                candidate = candidate3;
                direction = Vector2.up;
            }
            return candidate;
        }

        private void Generate(MeshGenerationContext context)
        {
            const float ArrowLength = 14f;

            var painter = context.painter2D;
            painter.BeginPath();
            painter.MoveTo(fromPoint);
            painter.LineTo(fromPoint + fromDir * ArrowLength / 1.414f);
            painter.BezierCurveTo(control0, control1, toPoint + toDir * ArrowLength / 1.414f);
            painter.LineTo(toPoint);
            painter.MoveTo(toPoint);
            var crossTo = new Vector2(toDir.y, -toDir.x);
            painter.LineTo(toPoint + (toDir + crossTo).normalized * ArrowLength);
            painter.MoveTo(toPoint);
            painter.LineTo(toPoint + (toDir - crossTo).normalized * ArrowLength);

            painter.lineCap = LineCap.Round;
            painter.lineWidth = LineWidth;
            painter.strokeColor = Color.white;
            painter.Stroke();
        }
    }
}
