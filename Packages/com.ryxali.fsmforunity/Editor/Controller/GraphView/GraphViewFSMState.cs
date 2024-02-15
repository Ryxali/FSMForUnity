using FSMForUnity.Editor.IMGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

namespace FSMForUnity.Editor
{
    internal class GraphViewFSMState : IFSMState
    {
        private readonly DebuggerFSMStateData stateData;
        private readonly VisualElement container;

        private readonly RepeatingBackgroundElement graphCanvas;
        private readonly VisualTreeAsset graphNodeAsset;
        private readonly ObjectPool<VisualElement> graphNodePool;
        private readonly ObjectPool<ConnectionVisualElement> graphConnectionPool;
        private readonly Texture gridTexture;
        private readonly MachineGraph machineGraph;

        private bool isPanning;
        private Vector2 heldPosition;
        private float zoomLevel;
        private const float UnitConvert = 250f;

        public GraphViewFSMState(DebuggerFSMStateData stateData, VisualElement container)
        {
            this.stateData = stateData;
            this.container = container;
            gridTexture = IMGUIUtil.GenerateRepeatingGridTexture(128, 2, new Color(0.2f, 0.2f, 0.2f, 2f), new Color(0.6f, 0.6f, 0.6f, 1f));
            gridTexture.hideFlags = HideFlags.HideAndDontSave;
            graphCanvas = new RepeatingBackgroundElement(gridTexture);
            graphNodeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIMap_GraphView.GraphNodePath);
            graphNodePool = new ObjectPool<VisualElement>(() => graphNodeAsset.Instantiate(), elem => { });
            graphConnectionPool = new ObjectPool<ConnectionVisualElement>(() => new ConnectionVisualElement(), elem => elem.Connect(default, default));
            machineGraph = new MachineGraph();
        }

        public void Enter()
        {
            machineGraph.Regenerate(stateData.currentlyInspecting);
            graphCanvas.Reset();
            container.Add(graphCanvas);
            foreach (var node in machineGraph.GetStates())
            {
                var elem = graphNodePool.Take();
                //var pos = elem.style.position;
                //pos.value = Position.Absolute;
                var translate = elem.style.translate;
                graphCanvas.Add(elem);
                var left = elem.style.left;
                var right = elem.style.right;
                translate.value = new Translate(new Length(container.contentRect.width/2 + node.position.x * UnitConvert), new Length(container.contentRect.height / 2 + node.position.y * UnitConvert), 0f);
                elem.style.translate = translate;
                //elem.transform.position = node.position;
            }
            foreach (var conn in machineGraph.GetTransitions())
            {
                var elem = graphConnectionPool.Take();
                graphCanvas.Add(elem);
                elem.Connect(new Rect(container.contentRect.size / 2 + conn.origin * UnitConvert, default), new Rect(container.contentRect.size / 2 + conn.destination * UnitConvert, default));
            }
            container.RegisterCallback<MouseDownEvent>(OnPanDown, TrickleDown.NoTrickleDown);
            container.RegisterCallback<MouseUpEvent>(OnPanUp, TrickleDown.NoTrickleDown);
            container.RegisterCallback<MouseMoveEvent>(OnPanDrag, TrickleDown.NoTrickleDown);
            container.RegisterCallback<WheelEvent>(OnZoom, TrickleDown.NoTrickleDown);
        }

        public void Exit()
        {
            container.UnregisterCallback<MouseDownEvent>(OnPanDown, TrickleDown.NoTrickleDown);
            container.UnregisterCallback<MouseUpEvent>(OnPanUp, TrickleDown.NoTrickleDown);
            container.UnregisterCallback<MouseMoveEvent>(OnPanDrag, TrickleDown.NoTrickleDown);
            container.UnregisterCallback<WheelEvent>(OnZoom, TrickleDown.NoTrickleDown);
            foreach (var elem in graphCanvas.Children())
            {
                if (elem is ConnectionVisualElement vElem)
                    graphConnectionPool.Return(vElem);
                else
                    graphNodePool.Return(elem);
            }
            graphCanvas.Clear();
            container.Remove(graphCanvas);
        }

        public void Update(float delta)
        {
        }

        void IFSMState.Destroy()
        {
            UnityEngine.Object.DestroyImmediate(gridTexture);
        }

        private void OnZoom(WheelEvent evt)
        {
            zoomLevel = Mathf.Clamp01(zoomLevel - evt.delta.y * 0.01f);
            graphCanvas.Zoom(Mathf.Lerp(1f, 10f, zoomLevel * zoomLevel), evt.localMousePosition);
        }

        private void OnPanDown(MouseDownEvent evt)
        {
            if (evt.button == 2)
            {
                isPanning = true;
                heldPosition = evt.mousePosition;
            }
        }
        private void OnPanUp(MouseUpEvent evt)
        {
            if (evt.button == 2)
            {
                isPanning = false;
            }
        }
        private void OnPanDrag(MouseMoveEvent evt)
        {
            if (isPanning)
            {
                var pos = evt.mousePosition;
                graphCanvas.Pan(pos - heldPosition);
                heldPosition = pos;
            }
        }
    }

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

    internal class TestMeshVisualElement : VisualElement
    {
        private readonly float margin;
        private readonly float bevelRadius;

        const float marginUv = 0.25f;

        public TestMeshVisualElement(float margin, float bevelRadius)
        {
            this.margin = margin;
            this.bevelRadius = bevelRadius;
            generateVisualContent = Generate;
            style.height = new StyleLength(250);
            style.width = new StyleLength(600);
            style.position = new StyleEnum<Position>(Position.Absolute);
            style.left = new StyleLength(100f);
            style.top = new StyleLength(100f);
            //style.color = new StyleColor(Color.red);
            //style.backgroundColor = new StyleColor(Color.white);
            // style.backgroundImage = new StyleBackground(Background.FromTexture2D(Texture2D.whiteTexture));
            //style.color = new StyleColor(Color.white);
        }

        private void Generate(MeshGenerationContext gen)
        {
            var rect = gen.visualElement.contentRect;
            if (margin > 0)
            {
                const int BevelVertices = 9;
                const int Vertices = 4 + BevelVertices * 4;
                int triCount = 2 + Mathf.Min(1, BevelVertices) * 2 * 4 + (BevelVertices-1) * 4;
                int indexCount = triCount * 3;

                var meshWrite = gen.Allocate(Vertices, indexCount);

                var center = IMGUI.IMGUIUtil.PadRect(rect, margin);

                var centerColor = gen.visualElement.style.color.value;
                var edgeColor = IMGUI.IMGUIUtil.Blend(new Color(0, 0, 0, 0.2f), centerColor);

                // center box
                meshWrite.SetNextVertex(new Vertex { position = center.min, uv = new Vector2(marginUv, marginUv), tint = centerColor });
                meshWrite.SetNextVertex(new Vertex { position = new Vector3(center.xMin, center.yMax), uv = new Vector2(marginUv, 1f - marginUv), tint = centerColor });
                meshWrite.SetNextVertex(new Vertex { position = center.max, uv = new Vector2(1f-marginUv, 1f-marginUv), tint = centerColor });
                meshWrite.SetNextVertex(new Vertex { position = new Vector3(center.xMax, center.yMin), uv = new Vector2(1f - marginUv, marginUv), tint = centerColor });
                meshWrite.SetNextIndex(0);
                meshWrite.SetNextIndex(2);
                meshWrite.SetNextIndex(1);
                meshWrite.SetNextIndex(0);
                meshWrite.SetNextIndex(3);
                meshWrite.SetNextIndex(2);


                var radRect = IMGUI.IMGUIUtil.PadRect(rect, bevelRadius);

                // bottom-left corner
                for (int i = 0; i < BevelVertices; i++)
                {
                    var vec = Quaternion.Euler(0, 0, 90f * (1f - i / (float)(BevelVertices - 1))) * new Vector3(-bevelRadius, 0);
                    var pos = (Vector3)radRect.min + vec;
                    pos.z = Vertex.nearZ;
                    Debug.Log($"{pos} : {vec}");
                    meshWrite.SetNextVertex(new Vertex { position = pos, uv = new Vector2(Mathf.InverseLerp(rect.xMin, rect.xMax, pos.x), Mathf.InverseLerp(rect.yMin, rect.yMax, pos.y)), tint = edgeColor });
                }
                // top-left corner
                for (int i = 0; i < BevelVertices; i++)
                {
                    var vec = Quaternion.Euler(0, 0, -90f + 90f *  (1f-i / (float)(BevelVertices - 1))) * new Vector3(-bevelRadius, 0);
                    var pos = new Vector3(radRect.xMin, radRect.yMax) + vec;
                    pos.z = Vertex.nearZ;
                    Debug.Log($"{pos} : {vec}");
                    meshWrite.SetNextVertex(new Vertex { position = pos, uv = new Vector2(Mathf.InverseLerp(rect.xMin, rect.xMax, pos.x), Mathf.InverseLerp(rect.yMin, rect.yMax, pos.y)), tint = edgeColor });
                }
                // top-right corner
                for (int i = 0; i < BevelVertices; i++)
                {
                    var vec = Quaternion.Euler(0, 0, -180f + 90f * (1f - i / (float)(BevelVertices - 1))) * new Vector3(-bevelRadius, 0);
                    var pos = (Vector3)radRect.max + vec;
                    pos.z = Vertex.nearZ;
                    Debug.Log($"{pos} : {vec}");
                    meshWrite.SetNextVertex(new Vertex { position = pos, uv = new Vector2(Mathf.InverseLerp(rect.xMin, rect.xMax, pos.x), Mathf.InverseLerp(rect.yMin, rect.yMax, pos.y)), tint = edgeColor });
                }
                // bottom-right corner
                for (int i = 0; i < BevelVertices; i++)
                {
                    var vec = Quaternion.Euler(0, 0, -270f + 90f * (1f - i / (float)(BevelVertices - 1))) * new Vector3(-bevelRadius, 0);
                    var pos = new Vector3(radRect.xMax, radRect.yMin) + vec;
                    pos.z = Vertex.nearZ;
                    Debug.Log($"{pos} : {vec}");
                    meshWrite.SetNextVertex(new Vertex { position = pos, uv = new Vector2(Mathf.InverseLerp(rect.xMin, rect.xMax, pos.x), Mathf.InverseLerp(rect.yMin, rect.yMax, pos.y)), tint = edgeColor });
                }

                for (ushort j = 0; j < 4; j++)
                {
                    for (int i = 0; i < BevelVertices; i++)
                    {
                        var currentVertIndex = 4 + i + j * BevelVertices;
                        if (i == 0)
                        {
                            meshWrite.SetNextIndex((ushort)currentVertIndex);
                            meshWrite.SetNextIndex((ushort)((j + 3) % 4));
                            meshWrite.SetNextIndex((ushort)(j));
                            meshWrite.SetNextIndex((ushort)currentVertIndex);
                            meshWrite.SetNextIndex((ushort)(j == 0 ? Vertices - 1 : currentVertIndex-1));
                            meshWrite.SetNextIndex((ushort)((j + 3) % 4)); // currentVertIndex + BevelVertices
                            Debug.Log($"({currentVertIndex},{Vertices - 1}, {0})");
                            //meshWrite.SetNextIndex((ushort)currentVertIndex);
                            //meshWrite.SetNextIndex((ushort)(j == 0 ? (Vertices - 1) : (currentVertIndex - 1)));
                            //meshWrite.SetNextIndex((ushort)(j == 0 ? 3 : j - 1));
                        }
                        //else if (i == BevelVertices - 1)
                        //{
                        //    meshWrite.SetNextIndex((ushort)currentVertIndex);
                        //    meshWrite.SetNextIndex((ushort)(j == 3 ? (4) : (currentVertIndex+1)));
                        //    meshWrite.SetNextIndex((ushort)(j));
                        //}
                        else
                        {
                            meshWrite.SetNextIndex((ushort)currentVertIndex);
                            meshWrite.SetNextIndex((ushort)(currentVertIndex - 1));
                            meshWrite.SetNextIndex((ushort)(j));
                        }
                    }
                }
            }
            else
            {
                var meshWrite = gen.Allocate(3, 3, Texture2D.whiteTexture);
                var uvRect = meshWrite.uvRegion;
                meshWrite.SetAllVertices(new[] {
                new Vertex { position = rect.min, uv = uvRect.min, tint = Color.white },
                new Vertex { position = new Vector3(rect.xMin, rect.yMax), uv =  new Vector2(uvRect.xMin, uvRect.yMax), tint =  Color.white },
                new Vertex { position = rect.max, uv = uvRect.max, tint = Color.white },
                //new Vertex { position = new Vector3(rect.xMax, rect.yMin, 0), uv = new Vector2(uvRect.xMax, uvRect.yMin), tint =  Color.white }
                });
                    meshWrite.SetAllIndices(new ushort[]{
                    0, 2, 1,
                    //2, 3, 1
                });
            }
        }
    }
}
