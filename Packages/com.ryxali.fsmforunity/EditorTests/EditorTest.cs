using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FSMForUnity.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EditorTest
{
    // A Test behaves as an ordinary method
    [Test]
    public void CalculateEdgesJobTest()
    {
        var nodes = new GraphNode[]
        {
            new GraphNode
            {
                position = new Vector2(0, 1)
            },
            new GraphNode
            {
                position = new Vector2(0, -1)
            }
        };
        var connections = new GraphConnection[]
        {
            new GraphConnection
            {
                origin = nodes[0],
                originIndex = 0,
                destination = nodes[1],
                destinationIndex = 1,
            }
        };
        CalculateEdgesJob.Solve(connections, 100, 100, out var edges).Complete();
        using (edges)
        {
            Assert.AreEqual(2, edges.Length);
            Assert.AreEqual(ConnectionEdge.Top, edges[0]);
            Assert.AreEqual(ConnectionEdge.Bottom, edges[1]);
        }
    }

    [Test]
    public void CalculateEdgesJobTest2()
    {
        var nodes = new GraphNode[]
        {
            new GraphNode
            {
                position = new Vector2(-1, 0)
            },
            new GraphNode
            {
                position = new Vector2(1, 0)
            }
        };
        var connections = new GraphConnection[]
        {
            new GraphConnection
            {
                origin = nodes[0],
                originIndex = 0,
                destination = nodes[1],
                destinationIndex = 1,
            }
        };
        CalculateEdgesJob.Solve(connections, 100, 100, out var edges).Complete();
        using (edges)
        {
            Assert.AreEqual(2, edges.Length);
            Assert.AreEqual(ConnectionEdge.Right, edges[0]);
            Assert.AreEqual(ConnectionEdge.Left, edges[1]);
        }
    }

    [Test]
    public void CalculateEdgesJobTest3()
    {
        var nodes = new GraphNode[]
        {
            new GraphNode
            {
                position = new Vector2(0, -100)
            },
            new GraphNode
            {
                position = new Vector2(-100, 0)
            },
            new GraphNode
            {
                position = new Vector2(100, 0)
            }
        };
        var connections = new GraphConnection[]
        {
            new GraphConnection
            {
                origin = nodes[0],
                originIndex = 0,
                destination = nodes[1],
                destinationIndex = 1,
            },
            new GraphConnection
            {
                origin = nodes[2],
                originIndex = 2,
                destination = nodes[0],
                destinationIndex = 0,
            },
            new GraphConnection
            {
                origin = nodes[1],
                originIndex = 1,
                destination = nodes[2],
                destinationIndex = 2,
            },new GraphConnection
            {
                origin = nodes[2],
                originIndex = 2,
                destination = nodes[1],
                destinationIndex = 1,
            }
        };
        CalculateEdgesJob.Solve(connections, 100, 100, out var edges).Complete();
        using (edges)
        {
            Debug.Log(string.Join(", ", edges));
            Assert.AreEqual(8, edges.Length);
            Assert.AreEqual(ConnectionEdge.Right, edges[0]);
            Assert.AreEqual(ConnectionEdge.Left, edges[1]);
        }
    }

    [Test]
    public void CountConnectionsJobTest()
    {
        var nodes = new GraphNode[]
        {
            new GraphNode
            {
                position = new Vector2(0, 1)
            },
            new GraphNode
            {
                position = new Vector2(0, -1)
            }
        };
        var connections = new GraphConnection[]
        {
            new GraphConnection
            {
                origin = nodes[0],
                originIndex = 0,
                destination = nodes[1],
                destinationIndex = 1,
            }
        };
        var handle = CalculateEdgesJob.Solve(connections, 100, 100, out var edges);
        handle = CountConnectionsForEdgesJob.Solve(connections, edges, out var stateConnections, handle);
        handle.Complete();
        using (edges)
        {
            using (stateConnections)
            {
                Debug.Log(string.Join(", ", edges));
                Debug.Log(string.Join(", ", stateConnections));
                Assert.AreEqual(16, stateConnections.Length);
                Assert.AreEqual(new[] 
                { 
                    0, 0, 0, 0, 1, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 1
                }, stateConnections);
            }
        }
    }

    [Test]
    public void IndexConnectionsJobTest()
    {
        var nodes = new GraphNode[]
        {
            new GraphNode
            {
                position = new Vector2(0, 1)
            },
            new GraphNode
            {
                position = new Vector2(0, -1)
            }
        };
        var connections = new GraphConnection[]
        {
            new GraphConnection
            {
                origin = nodes[0],
                originIndex = 0,
                destination = nodes[1],
                destinationIndex = 1,
            }
        };
        var handle = CalculateEdgesJob.Solve(connections, 100, 100, out var edges);
        handle = CountConnectionsForEdgesJob.Solve(connections, edges, out var stateConnections, handle);
        handle = IndexConnectionsJob.Solve(edges, connections, stateConnections, out var indices, handle);
        handle.Complete();
        using (edges)
        {
            using (stateConnections)
            {
                using (indices)
                {
                    Debug.Log(string.Join(", ", edges));
                    Debug.Log(string.Join(", ", stateConnections));
                    Debug.Log(string.Join(", ", indices.Select(i => $"{i.index}/{i.count}")));
                    Assert.AreEqual(new[] { new ConnectionCount { count = 1, index = 0 }, new ConnectionCount { count = 1, index = 0 } }, indices);
                }
            }
        }
    }

    [Test]
    public void IndexConnectionsJobComplexTest()
    {
        var nodes = new GraphNode[]
        {
            new GraphNode
            {
                position = new Vector2(0, 1)
            },
            new GraphNode
            {
                position = new Vector2(0, -1)
            },
            new GraphNode
            {
                position = new Vector2(0, 0)
            }
        };
        var connections = new GraphConnection[]
        {
            new GraphConnection
            {
                origin = nodes[0],
                originIndex = 0,
                destination = nodes[1],
                destinationIndex = 1,
            },
            new GraphConnection
            {
                origin = nodes[0],
                originIndex = 0,
                destination = nodes[2],
                destinationIndex = 2,
            }
        };
        var handle = CalculateEdgesJob.Solve(connections, 100, 100, out var edges);
        handle = CountConnectionsForEdgesJob.Solve(connections, edges, out var stateConnections, handle);
        handle = IndexConnectionsJob.Solve(edges, connections, stateConnections, out var indices, handle);
        handle.Complete();
        using (edges)
        {
            using (stateConnections)
            {
                using (indices)
                {
                    Debug.Log(string.Join(", ", edges));
                    Debug.Log(string.Join(", ", stateConnections));
                    Debug.Log(string.Join(", ", indices.Select(i => $"{i.index}/{i.count}")));
                    //Assert.AreEqual(new[] { new ConnectionCount { count = 2, index = 1 }, new ConnectionCount { count = 2, index = 1 } }, indices.ToArray());
                }
            }
        }
    }
}
