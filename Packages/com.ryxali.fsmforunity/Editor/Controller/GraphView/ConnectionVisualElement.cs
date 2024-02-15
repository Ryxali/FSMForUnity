using UnityEngine;
using UnityEngine.UIElements;

namespace FSMForUnity.Editor
{
    internal class ConnectionVisualElement : VisualElement
    {

        private Vector2 from, to;
        private float LineWidth = 5f;

        public ConnectionVisualElement()
        {
            var pos = style.position;
            style.position = new StyleEnum<Position>(Position.Absolute);
            generateVisualContent = Generate;
        }

        public void Connect(Rect from, Rect to)
        {
            var rect = Rect.MinMaxRect(Mathf.Min(from.center.x, to.center.x), Mathf.Min(from.center.y, to.center.y), Mathf.Max(from.center.x, to.center.x), Mathf.Max(from.center.y, to.center.y));
            rect.x -= LineWidth;
            rect.y -= LineWidth;
            rect.width += LineWidth * 2;
            rect.height += LineWidth * 2;
            style.translate = new StyleTranslate(new Translate(new Length(rect.x), new Length(rect.y), 0f));
            style.width = new StyleLength(new Length(rect.width));
            style.height = new StyleLength(new Length(rect.height));
            this.from = from.center - rect.position;
            this.to = to.center - rect.position;
            MarkDirtyRepaint();
        }

        private void Generate(MeshGenerationContext context)
        {
            var painter = context.painter2D;
            painter.BeginPath();
            var rect = context.visualElement.contentRect;
            painter.MoveTo(from);
            painter.LineTo(to);

            painter.lineCap = LineCap.Round;
            painter.lineWidth = LineWidth;
            painter.strokeColor = Color.white;
            painter.Stroke();
        }
    }
}
