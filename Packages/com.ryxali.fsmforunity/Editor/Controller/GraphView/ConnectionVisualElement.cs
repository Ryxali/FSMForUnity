using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace FSMForUnity.Editor
{
    internal class ConnectionVisualElement : VisualElement
    {
        private static CustomStyleProperty<Color> fgColorProp = new CustomStyleProperty<Color>("--fsmforunity-arrow-arrowcolor");
        private static CustomStyleProperty<Color> bgColorProp = new CustomStyleProperty<Color>("--fsmforunity-arrow-arrowoutlinecolor");
        private static readonly Color defaultFgColor = new Color32(0xff, 0xff, 0xff, 0xff);
        private static readonly Color defaultBgColor = new Color32(0x00, 0x00, 0x00, 0x00);

        public float Scale { get; set; } = 1f;
        private float LineWidth = 2f;

        private VisualElement from, to;

        private Rect fromRect, toRect;

        private Vector2 fromPoint, toPoint, control0, control1, fromDir, toDir;
        private ConnectionEdge fromEdge, toEdge;
        private float fromDelta, toDelta;
        private Color fgColor = defaultFgColor;
        private Color bgColor = defaultBgColor;

        public ConnectionVisualElement()
        {
            style.position = new StyleEnum<Position>(Position.Absolute);
            generateVisualContent = Generate;

            RegisterCallback<CustomStyleResolvedEvent>(OnStylesResolved);
            AddToClassList("fsmforunity-arrow");
        }

        private void OnStylesResolved(CustomStyleResolvedEvent evt)
        {
            fgColor = evt.customStyle.TryGetValue(fgColorProp, out var fgV) ? fgV : defaultFgColor;
            bgColor = evt.customStyle.TryGetValue(bgColorProp, out var bgV) ? bgV : defaultBgColor;
            MarkDirtyRepaint();
        }

        public void Connect(VisualElement from, ConnectionEdge fromEdge, float fromDelta, VisualElement to, ConnectionEdge toEdge, float toDelta)
        {
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
            var lineWidth = LineWidth * Scale;
            rect.x -= lineWidth;
            rect.y -= lineWidth;
            rect.width += lineWidth * 2;
            rect.height += lineWidth * 2;
            style.left = new StyleLength(new Length(rect.x));
            style.top = new StyleLength(new Length(rect.y));
            style.width = new StyleLength(new Length(rect.width));
            style.height = new StyleLength(new Length(rect.height));
            fromPoint = from - rect.position + fromDir * lineWidth * 0.5f;//from.center - rect.position;
            toPoint = to - rect.position + toDir * lineWidth* 0.5f;// to.center - rect.position;
            var len = Vector2.Distance(from, to) * 0.35f;
            control0 = fromPoint + fromDir * len;
            control1 = toPoint + toDir * len;
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
            const float ArrowLength = 4f;
            float arrowLength = ArrowLength * Scale;
            const float ArrowAngling = .3f;
            var crossTo = new Vector2(toDir.y, -toDir.x);
            var arr = (toDir + crossTo * ArrowAngling).normalized * arrowLength;
            var arrLen = Vector3.Dot(toDir, arr) * 1.5f;

            var painter = context.painter2D;
            painter.BeginPath();
            painter.MoveTo(fromPoint);
            painter.LineTo(fromPoint + fromDir * arrLen);
            painter.BezierCurveTo(control0, control1, toPoint + toDir * arrLen);
            painter.LineTo(toPoint);

            painter.lineCap = LineCap.Round;
            painter.lineWidth = LineWidth * Scale;
            painter.strokeColor = bgColor;
            painter.Stroke();

            painter.BeginPath();
            painter.MoveTo(toPoint);
            painter.LineTo(toPoint + (toDir + crossTo * ArrowAngling).normalized * arrowLength);
            painter.LineTo(toPoint + (toDir - crossTo * ArrowAngling).normalized * arrowLength);
            painter.ClosePath();

            painter.lineCap = LineCap.Round;
            painter.lineWidth = LineWidth * Scale;
            painter.strokeColor = bgColor;
            painter.fillColor = bgColor;
            painter.Fill();
            painter.Stroke();
            painter.lineWidth = LineWidth * Scale * 0.7f;
            painter.strokeColor = fgColor;
            painter.fillColor = fgColor;
            painter.Fill();
            painter.Stroke();

            painter.BeginPath();
            painter.MoveTo(fromPoint);
            painter.LineTo(fromPoint + fromDir * arrLen);
            painter.BezierCurveTo(control0, control1, toPoint + toDir * arrLen);
            painter.LineTo(toPoint);

            painter.lineWidth = LineWidth * Scale * 0.7f;
            painter.strokeColor = fgColor;
            painter.Stroke();
        }
    }
}
