using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace FSMForUnity.Editor
{

    internal class ConnectionVisualElement : VisualElement
    {
        public float Scale { get => scale; set { scale = value; OnGeometryUpdated(null); } }
        private float scale = 1f;

        private static readonly StyleWithDefault<Color> fgColorProp = new StyleWithDefault<Color>("--fsmforunity-arrow-arrowcolor", new Color32(0xff, 0xff, 0xff, 0xff));
        private static readonly StyleWithDefault<Color> bgColorProp = new StyleWithDefault<Color>("--fsmforunity-arrow-arrowoutlinecolor", new Color32(0x00, 0x00, 0x00, 0x00));
        private static readonly StyleWithDefault<float> thicknessProp = new StyleWithDefault<float>("--fsmforunity-arrow-thickness", 2f);
        private static readonly StyleWithDefault<float> outlineThicknessProp = new StyleWithDefault<float>("--fsmforunity-arrow-outlinethickness", 0.3f);
        private static readonly StyleWithDefault<float> headLengthProp = new StyleWithDefault<float>("--fsmforunity-arrow-headlength", 4f);
        private static readonly StyleWithDefault<float> headWidthProp = new StyleWithDefault<float>("--fsmforunity-arrow-headwidth", 2f);

        private readonly Label label;

        private VisualElement from, to;
        private Rect fromRect, toRect;
        private Vector2 fromPoint, toPoint, control0, control1, fromDir, toDir;
        private ConnectionEdge fromEdge, toEdge;
        private float fromDelta, toDelta;

        private Color fgColor = fgColorProp.defaultValue;
        private Color bgColor = bgColorProp.defaultValue;
        private float arrowThickness = thicknessProp.defaultValue;
        private float arrowOutlineThickness = 1f - outlineThicknessProp.defaultValue;
        private float arrowHeadLength = headLengthProp.defaultValue;
        private float arrowHeadWidth = headWidthProp.defaultValue;

        public ConnectionVisualElement()
        {
            style.position = new StyleEnum<Position>(Position.Absolute);
            generateVisualContent = Generate;
            label = new Label();
            Add(label);
            label.style.position = new StyleEnum<Position>(Position.Absolute);
            label.style.textOverflow = new StyleEnum<TextOverflow>(TextOverflow.Ellipsis);
            label.style.whiteSpace = WhiteSpace.NoWrap;
            label.style.overflow = Overflow.Hidden;
            label.style.unityTextAlign = TextAnchor.LowerCenter;
            RegisterCallback<CustomStyleResolvedEvent>(OnStylesResolved);
            RegisterCallback<GeometryChangedEvent>(OnGeometryUpdated);
            AddToClassList("fsmforunity-arrow");
            label.AddToClassList("fsmforunity-arrow-label");
        }

        private void OnStylesResolved(CustomStyleResolvedEvent evt)
        {
            fgColor = evt.customStyle.ValueOrDefault(fgColorProp);
            bgColor = evt.customStyle.ValueOrDefault(bgColorProp);
            arrowThickness = evt.customStyle.ValueOrDefault(thicknessProp);
            arrowOutlineThickness = 1f - evt.customStyle.ValueOrDefault(outlineThicknessProp);
            arrowHeadLength = evt.customStyle.ValueOrDefault(headLengthProp);
            arrowHeadWidth = evt.customStyle.ValueOrDefault(headWidthProp);
            MarkDirtyRepaint();
        }

        public void Connect(string label, VisualElement from, ConnectionEdge fromEdge, float fromDelta, VisualElement to, ConnectionEdge toEdge, float toDelta)
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
            this.label.text = label;
            
            RecalculateLayout();
        }

        public void OnGeometryUpdated(GeometryChangedEvent evt)
        {
            // text
            var point = Bezier(fromPoint + fromDir * arrowHeadLength * Scale, control0, control1, toPoint + toDir * arrowHeadLength * Scale, 0.5f);
            var dir = BezierTan(fromPoint + fromDir * arrowHeadLength * Scale, control0, control1, toPoint + toDir * arrowHeadLength * Scale, 0.5f).normalized;
            var labelWidth = label.contentRect.width;
            var dirX = new Vector2(dir.y, -dir.x);
            if (dirX.y > 0)
                dirX *= -1f;
            var corner = point + dirX * (label.contentRect.height + arrowThickness * Scale);
            label.style.left = new StyleLength(corner.x);
            label.style.top = new StyleLength(corner.y);
            label.style.translate = new StyleTranslate(new Translate(-labelWidth * 0.5f, 0, 0));
            label.style.width = 50f * Scale;
            label.style.width = 50f * Scale;
            var angle = Vector2.SignedAngle(Vector2.right, dir);
            if (angle > 90f) angle -= 180f;
            else if (angle < -90f)
                angle += 180f;
            label.style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
            label.MarkDirtyRepaint();
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
            var from = Offset(fromRect, fromEdge, Mathf.Lerp(0.25f, 1f, fromDelta));
            var to = Offset(toRect, toEdge, -Mathf.Lerp(0.25f, 1f, toDelta));
            fromDir = Dir(fromEdge);
            toDir = Dir(toEdge);
            var rect = Rect.MinMaxRect(Mathf.Min(from.x, to.x), Mathf.Min(from.y, to.y), Mathf.Max(from.x, to.x), Mathf.Max(from.y, to.y));
            var lineWidth = arrowThickness * Scale;
            rect.x -= lineWidth;
            rect.y -= lineWidth;
            rect.width += lineWidth * 2;
            rect.height += lineWidth * 2;
            style.left = new StyleLength(new Length(rect.x));
            style.top = new StyleLength(new Length(rect.y));
            style.width = new StyleLength(new Length(rect.width));
            style.height = new StyleLength(new Length(rect.height));
            fromPoint = from - rect.position + fromDir * lineWidth * 0.5f;
            toPoint = to - rect.position + toDir * lineWidth* 0.5f;
            var len = Vector2.Distance(from, to) * 0.35f;
            control0 = fromPoint + fromDir * len;
            control1 = toPoint + toDir * len;

        }

        private static Vector2 Bezier(Vector2 from, Vector2 control0, Vector2 control1, Vector2 to, float delta)
        {
            var deltaNeg = 1 - delta;
            return 
                deltaNeg * deltaNeg * deltaNeg * from 
                + 3 * delta * (deltaNeg * deltaNeg) * control0 
                + 3 * (delta * delta) * deltaNeg * control1 
                + delta * delta * delta * to;
        }

        private static Vector2 BezierTan(Vector2 from, Vector2 control0, Vector2 control1, Vector2 to, float delta)
        {
            //-3(1-t)^2 * P0 + 3(1-t)^2 * P1 - 6t(1-t) * P1 - 3t^2 * P2 + 6t(1-t) * P2 + 3t^2 * P3 
            var deltaNeg = 1 - delta;

            return 
                - 3 * deltaNeg * deltaNeg * from
                + 3 * deltaNeg * deltaNeg * control0
                - 6 * delta * deltaNeg * control0
                - 3 * delta * delta * control1
                + 6 * delta * deltaNeg * control1
                + 3 * delta * delta * to;
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

            var size = Mathf.Abs(Vector2.Dot(dir, rect.size*0.5f));
            output += dir * size * delta; 
            return output;
        }

        private void Generate(MeshGenerationContext context)
        {
            float arrowLength = arrowHeadLength * Scale;
            float arrowWidth = arrowHeadWidth * Scale;
            var crossTo = new Vector2(toDir.y, -toDir.x);

            var painter = context.painter2D;
            painter.BeginPath();
            painter.MoveTo(fromPoint);
            painter.LineTo(fromPoint + fromDir * arrowLength);
            painter.BezierCurveTo(control0, control1, toPoint + toDir * arrowLength);
            painter.LineTo(toPoint);

            painter.lineCap = LineCap.Round;
            painter.lineWidth = arrowThickness * Scale;
            painter.strokeColor = bgColor;
            painter.Stroke();

            painter.BeginPath();
            painter.MoveTo(toPoint);
            painter.LineTo(toPoint + toDir * arrowLength + crossTo * arrowWidth);
            painter.LineTo(toPoint + toDir * arrowLength - crossTo * arrowWidth);
            painter.ClosePath();

            painter.lineCap = LineCap.Round;
            painter.lineWidth = arrowThickness * Scale;
            painter.strokeColor = bgColor;
            painter.fillColor = bgColor;
            painter.Fill();
            painter.Stroke();
            painter.lineWidth = arrowThickness * Scale * arrowOutlineThickness;
            painter.strokeColor = fgColor;
            painter.fillColor = fgColor;
            painter.Stroke();
            painter.Fill();

            painter.BeginPath();
            painter.MoveTo(fromPoint);
            painter.LineTo(fromPoint + fromDir * arrowLength);
            painter.BezierCurveTo(control0, control1, toPoint + toDir * arrowLength);
            //painter.LineTo(toPoint);

            painter.lineWidth = arrowThickness * Scale * arrowOutlineThickness;
            painter.strokeColor = fgColor;
            painter.Stroke();
        }
    }
}
