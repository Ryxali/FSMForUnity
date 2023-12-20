using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace FSMForUnity.Editor
{
    internal class GraphViewFSMState : IFSMState
    {
        private readonly DebuggerFSMStateData stateData;
        private readonly VisualElement container;

        private readonly VisualElement testGeneratedMesh;

        public GraphViewFSMState(DebuggerFSMStateData stateData, VisualElement container)
        {
            this.stateData = stateData;
            this.container = container;
            testGeneratedMesh = new TestMeshVisualElement(30f, 10f);
        }

        public void Enter()
        {
            container.Add(testGeneratedMesh);
        }

        public void Exit()
        {
        }

        public void Update(float delta)
        {
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
            style.color = new StyleColor(Color.red);
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
