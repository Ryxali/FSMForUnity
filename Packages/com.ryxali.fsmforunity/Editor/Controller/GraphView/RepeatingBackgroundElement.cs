using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UIElements;

namespace FSMForUnity.Editor
{
    internal class RepeatingBackgroundElement : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<RepeatingBackgroundElement, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits { }

        private static readonly Color defaultFgColor = new Color32(0x14, 0x14, 0x14, 0xff);
        private static readonly Color defaultBgColor = new Color32(0x1c, 0x1c, 0x1c, 0xff);
        private static readonly int defaultThickness = 2;
        private static readonly int defaultSize = 128;
        private const string ModeGrid = "grid";
        private const string ModePoints = "points";
        private static CustomStyleProperty<Color> bgColorProp = new CustomStyleProperty<Color>("--fsmforunity-grid-bgcolor");
        private static CustomStyleProperty<Color> fgColorProp = new CustomStyleProperty<Color>("--fsmforunity-grid-fgcolor");
        private static CustomStyleProperty<int> thicknessProp = new CustomStyleProperty<int>("--fsmforunity-grid-thickness");
        private static CustomStyleProperty<int> sizeProp = new CustomStyleProperty<int>("--fsmforunity-grid-size");
        private static CustomStyleProperty<string> modeProp = new CustomStyleProperty<string>("--fsmforunity-grid-mode");

        private Texture2D texture;

        public Vector2 offset { get; private set; }
        public float zoom { get; private set; }

        public RepeatingBackgroundElement()
        {
            this.texture = new Texture2D(defaultSize, defaultSize, TextureFormat.ARGB32, true);
            this.texture.hideFlags = HideFlags.HideAndDontSave;
            RegenerateGridTexture(defaultThickness, defaultFgColor, defaultBgColor);
            generateVisualContent = Generate;

            style.height = new StyleLength(new Length(100f, LengthUnit.Percent));
            style.width = new StyleLength(new Length(100f, LengthUnit.Percent));
            style.position = new StyleEnum<Position>(Position.Relative);
            zoom = 1f;
            AddToClassList("fsmforunity-gridview");
            RegisterCallback<CustomStyleResolvedEvent>(OnStylesResolved);
        }

        private void OnStylesResolved(CustomStyleResolvedEvent evt)
        {
            var thickness = evt.customStyle.TryGetValue(thicknessProp, out var v) ? v : defaultThickness;
            var lineColor = evt.customStyle.TryGetValue(fgColorProp, out var fgV) ? fgV : defaultFgColor;
            var backgroundColor = evt.customStyle.TryGetValue(bgColorProp, out var bgV) ? bgV : defaultBgColor;
            var texSize = evt.customStyle.TryGetValue(sizeProp, out var sizeV) ? Mathf.Max(1, sizeV) : defaultSize;
            var mode = evt.customStyle.TryGetValue(modeProp, out var modeV) ? modeV : ModeGrid;
            if (texture.width != texSize)
            {
                Object.DestroyImmediate(texture);
                texture = new Texture2D(texSize, texSize, TextureFormat.ARGB32, true);
            }
            switch(mode)
            {
                case ModePoints:
                    RegeneratePointsTexture(thickness, lineColor, backgroundColor);
                    break;
                case ModeGrid:
                default:
                    RegenerateGridTexture(thickness, lineColor, backgroundColor);
                    break;
            }
        }

        private void RegenerateGridTexture(int thickness, Color32 lineColor, Color32 backgroundColor)
        {

            var texGen = new FillGridTexture(texture, thickness, lineColor, backgroundColor, out var textureColors);
            var handle = texGen.Schedule(textureColors.Length, 128);
            handle.Complete();
            texture.SetPixelData(textureColors, 0);
            texture.Apply(true);
            textureColors.Dispose();
            MarkDirtyRepaint();
        }

        private void RegeneratePointsTexture(int thickness, Color32 lineColor, Color32 backgroundColor)
        {

            var texGen = new FillPointsTexture(texture, thickness, lineColor, backgroundColor, out var textureColors);
            var handle = texGen.Schedule(textureColors.Length, 128);
            handle.Complete();
            texture.SetPixelData(textureColors, 0);
            texture.Apply(true);
            textureColors.Dispose();
            MarkDirtyRepaint();
        }

        public void Dispose()
        {
            Object.DestroyImmediate(texture);
        }

        public void Reset()
        {
            offset = Vector2.zero;
            zoom = 1f;
            MarkDirtyRepaint();
        }

        public void Pan(Vector2 delta)
        {
            offset += delta;
            MarkDirtyRepaint();
        }


        public void Zoom(float zoomLevel, Vector2 towards)
        {
            const float MinZoom = 1f;

            var center = towards;

            var lP = offset - center;

            zoomLevel = Mathf.Max(zoomLevel, MinZoom);
            var prev = zoom;
            zoom = zoomLevel;

            offset = center + lP * zoom / prev;
            MarkDirtyRepaint();
        }

        private void Generate(MeshGenerationContext context)
        {
            float Tiling = texture.width / 8f;

            var zoomedTiling = Tiling * zoom;

            var cellOffset = -new Vector2((offset.x+ zoomedTiling) % zoomedTiling, (offset.y+ zoomedTiling) % zoomedTiling);
            var rect = context.visualElement.contentRect;
            rect.position -= cellOffset;
            rect.size += cellOffset;

            var cellJob = new CalculateCells(rect, zoomedTiling, out var cells);
            var handle = cellJob.Schedule();


            var meshWrite = new GenerateData(cells, zoomedTiling, context, texture, out var writeData, out var vertices, out var indices);
            handle = meshWrite.Schedule(cells.Length, 8, handle);
            
            handle.Complete();

            writeData.SetAllVertices(vertices);
            writeData.SetAllIndices(indices);
            cells.Dispose();
            vertices.Dispose();
            indices.Dispose();
        }

        private struct FillGridTexture : IJobParallelFor
        {
            private readonly int width;
            private readonly int thickness;
            private readonly Color32 lineColor;
            private readonly Color32 backgroundColor;
            [WriteOnly]
            private NativeArray<byte> output;
            
            public FillGridTexture(Texture2D texture, int thickness, Color32 lineColor, Color32 backgroundColor, out NativeArray<byte> output)
            {
                width = texture.width;
                this.thickness = thickness;
                this.lineColor = lineColor;
                this.backgroundColor = backgroundColor;
                this.output = output = new NativeArray<byte>(texture.width * texture.height * 4, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            }

            public void Execute(int index)
            {
                var pixelIndex = index / 4;
                var x = pixelIndex % width;
                var y = pixelIndex / width;
                var color = y < thickness || x < thickness ? lineColor : backgroundColor;
                switch(index % 4)
                {
                    case 0:
                        output[index] = color.a;
                        break;
                    case 1:
                        output[index] = color.r;
                        break;
                    case 2:
                        output[index] = color.g;
                        break;
                    case 3:
                        output[index] = color.b;
                        break;
                }
            }
        }

        private struct FillPointsTexture : IJobParallelFor
        {
            private readonly int width;
            private readonly int thickness;
            private readonly Color32 lineColor;
            private readonly Color32 backgroundColor;
            [WriteOnly]
            private NativeArray<byte> output;

            public FillPointsTexture(Texture2D texture, int thickness, Color32 lineColor, Color32 backgroundColor, out NativeArray<byte> output)
            {
                width = texture.width;
                this.thickness = thickness;
                this.lineColor = lineColor;
                this.backgroundColor = backgroundColor;
                this.output = output = new NativeArray<byte>(texture.width * texture.height * 4, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            }

            public void Execute(int index)
            {
                var pixelIndex = index / 4;
                var x = pixelIndex % width;
                var y = pixelIndex / width;
                var vec = new Vector2(x, y);
                var center = new Vector2(width / 2, width / 2);
                var dist =  Vector2.Distance(vec, center) / thickness;
                var color = Color32.Lerp(lineColor, backgroundColor, dist < 0.9f ? 0f : Mathf.Clamp(dist, 0.9f, 1f));
                // var color = y < thickness || x < thickness ? lineColor : backgroundColor;
                switch (index % 4)
                {
                    case 0:
                        output[index] = color.a;
                        break;
                    case 1:
                        output[index] = color.r;
                        break;
                    case 2:
                        output[index] = color.g;
                        break;
                    case 3:
                        output[index] = color.b;
                        break;
                }
            }
        }

        private struct CalculateCells : IJob
        {
            [WriteOnly]
            private NativeArray<Vector2> cellOrigins;
            private readonly Rect rect;
            private readonly float tiling;
            private readonly int width;
            private readonly int height;

            public CalculateCells(Rect rect, float tiling, out NativeArray<Vector2> cellOrigins)
            {
                this.rect = rect;
                this.rect.position -= new Vector2(tiling, tiling);
                this.rect.size += new Vector2(tiling, tiling);
                this.tiling = tiling;
                width = Mathf.CeilToInt(rect.width / tiling) + 1;
                height = Mathf.CeilToInt(rect.height / tiling) + 1;
                this.cellOrigins = cellOrigins = new NativeArray<Vector2>(width * height, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            }
            public void Execute()
            {
                var origin = rect.min;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var index = x + y * width;

                        var pos = origin + Vector2.right * tiling * x + Vector2.up * tiling * y;
                        cellOrigins[index] = pos + Vector2.one * tiling * 0.5f;
                    }
                }

            }
        }

        private struct GenerateData : IJobParallelFor
        {
            [ReadOnly]
            [NativeDisableParallelForRestriction]
            private NativeArray<Vector2> cellOrigins;
            [WriteOnly]
            [NativeDisableParallelForRestriction]
            private NativeArray<Vertex> vertices;
            [WriteOnly]
            [NativeDisableParallelForRestriction]
            private NativeArray<ushort> indices;

            private readonly float tiling;
            private readonly Rect uvRegion;
            private readonly Color32 color;
            public GenerateData(NativeArray<Vector2> cellOrigins, float tiling, MeshGenerationContext context, Texture texture, out MeshWriteData writeData, out NativeArray<Vertex> vertices, out NativeArray<ushort> indices)
            {
                this.cellOrigins = cellOrigins;
                this.tiling = tiling;
                color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                this.vertices = vertices = new NativeArray<Vertex>(5 * cellOrigins.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                this.indices = indices = new NativeArray<ushort>(cellOrigins.Length * 12, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                writeData = context.Allocate(vertices.Length, indices.Length, texture);
                uvRegion = writeData.uvRegion;
            }

            public void Execute(int index)
            {
                var cell = cellOrigins[index];

                var vIndex = index * 5;

                vertices[vIndex] = new Vertex { position = PositionOnElement(cell, 0, 0), uv = UVFromRegion(0,0), tint = color };
                vertices[vIndex+1] = new Vertex { position = PositionOnElement(cell, 0, 1), uv = UVFromRegion(0,1), tint = color };
                vertices[vIndex+2] = new Vertex { position = PositionOnElement(cell, 1, 1), uv = UVFromRegion(0,0), tint = color };
                vertices[vIndex+3] = new Vertex { position = PositionOnElement(cell, 1, 0), uv = UVFromRegion(1,0), tint = color };
                vertices[vIndex+4] = new Vertex { position = PositionOnElement(cell, .5f, .5f), uv = UVFromRegion(.5f,.5f), tint = color };

                var iIndex = index * 12;

                indices[iIndex] = (ushort)(vIndex + 0);
                indices[iIndex + 1] = (ushort)(vIndex + 4);
                indices[iIndex + 2] = (ushort)(vIndex + 1);

                indices[iIndex + 3] = (ushort)(vIndex + 1);
                indices[iIndex + 4] = (ushort)(vIndex + 4);
                indices[iIndex + 5] = (ushort)(vIndex + 2);

                indices[iIndex + 6] = (ushort)(vIndex + 2);
                indices[iIndex + 7] = (ushort)(vIndex + 4);
                indices[iIndex + 8] = (ushort)(vIndex + 3);

                indices[iIndex + 9] = (ushort)(vIndex + 3);
                indices[iIndex + 10] = (ushort)(vIndex + 4);
                indices[iIndex + 11] = (ushort)(vIndex + 0);
            }

            private Vector2 UVFromRegion(float x, float y)
            {
                return uvRegion.min + new Vector2(uvRegion.width * x, uvRegion.height * y);
            }
            private Vector3 PositionOnElement(Vector2 cell, float x, float y)
            {
                return new Vector3(cell.x + tiling * (x-0.5f), cell.y + tiling * (y-0.5f), Vertex.nearZ);
            }
        }
    }
}
