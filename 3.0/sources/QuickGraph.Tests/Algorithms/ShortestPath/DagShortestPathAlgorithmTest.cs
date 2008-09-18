using System;
using System.Collections.Generic;
using QuickGraph.Unit;
using QuickGraph.Algorithms.Observers;
using Microsoft.Pex.Framework;

namespace QuickGraph.Algorithms.ShortestPath
{
    [TestFixture, PexClass]
    public partial class DagShortestPathAlgorithmTest
    {
        [PexMethod]
        public void Compute(IVertexListGraph<string, Edge<string>> g)
        {
            // is this a dag ?
            bool isDag = g.IsDirectedAcyclicGraph();

            IDistanceRelaxer relaxer = ShortestDistanceRelaxer.Instance;
            List<string> vertices = new List<string>(g.Vertices);
            foreach (string root in vertices)
            {
                if (isDag)
                    Search(g, root, relaxer);
                else
                {
                    try
                    {
                        Search(g, root, relaxer);
                    }
                    catch (NonAcyclicGraphException)
                    {
                        Console.WriteLine("NonAcyclicGraphException caught (as expected)");
                    }
                }
            }
        }

        [Test]
        public void Simple()
        {
            AdjacencyGraph<string, Edge<string>> g = new AdjacencyGraph<string, Edge<string>>();
            GraphFactory.Simple(g);
            this.Compute(g);
            this.ComputeCriticalPath(g);
        }

        [Test]
        public void FileDependency()
        {
            AdjacencyGraph<string, Edge<string>> g = new AdjacencyGraph<string, Edge<string>>();
            GraphFactory.FileDependency(g);
            this.Compute(g);
            this.ComputeCriticalPath(g);
        }

        [PexMethod]
        public void ComputeCriticalPath(IVertexListGraph<string, Edge<string>> g)
        {
            // is this a dag ?
            bool isDag = g.IsDirectedAcyclicGraph();

            var relaxer = CriticalDistanceRelaxer.Instance;
            var vertices = new List<string>(g.Vertices);
            foreach (string root in vertices)
            {
                if (isDag)
                    Search(g, root, relaxer);
                else
                {
                    try
                    {
                        Search(g, root, relaxer);
                        Assert.Fail("should have found the acyclic graph");
                    }
                    catch (NonAcyclicGraphException)
                    {
                        Console.WriteLine("NonAcyclicGraphException caught (as expected)");
                    }
                }
            }
        }

        private void Search(
            IVertexListGraph<string, Edge<string>> g, 
            string root, IDistanceRelaxer relaxer)
        {
            var algo = 
                new DagShortestPathAlgorithm<string, Edge<string>>(
                    g,
                    e => 1,
                    relaxer
                    );
            var predecessors = new VertexPredecessorRecorderObserver<string, Edge<string>>();
            using(ObserverScope.Create(algo, predecessors))
                algo.Compute(root);

            Verify(algo, predecessors);
        }

        private static void Verify(DagShortestPathAlgorithm<string, Edge<string>> algo, VertexPredecessorRecorderObserver<string, Edge<string>> predecessors)
        {
            // let's verify the result
            foreach (string v in algo.VisitedGraph.Vertices)
            {
                Edge<string> predecessor;
                if (!predecessors.VertexPredecessors.TryGetValue(v, out predecessor))
                    continue;
                if (predecessor.Source == v)
                    continue;
                Assert.AreEqual(
                    algo.Distances[v], algo.Distances[predecessor.Source] + 1
                    );
            }
        }
    }
}
