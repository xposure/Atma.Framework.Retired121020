namespace Atma.Common
{
    using System.Linq;
    using Shouldly;
    using Xunit;

    public class DirectedGraphTests
    {
        [Fact]
        public void ShouldFindPath()
        {
            //https://eli.thegreenplace.net/2015/directed-graph-traversal-orderings-and-applications-to-data-flow-analysis/
            var graph = new DirectedGraph<char>();
            graph.AddNodes('x', 't', 'c', 'b', 'e', 'd', 'r', 'p', 'j', 'f', 'k', 'q');
            graph.AddEdge('x', 't');
            graph.AddEdge('x', 'b');
            graph.AddEdge('x', 'c');

            graph.AddEdge('t', 'b');

            graph.AddEdge('b', 'd');

            graph.AddEdge('c', 'b');
            graph.AddEdge('c', 'e');

            graph.AddEdge('e', 'd');

            graph.AddEdge('f', 'j');

            graph.AddEdge('q', 'k');
            graph.AddEdge('q', 'p');

            graph.AddEdge('k', 'r');

            graph.AddEdge('p', 'r');


            graph.Roots.Count().ShouldBe(3);

            graph.ReversePostOrder('x').ShouldBe(new char[] { 'd', 'b', 't', 'e', 'c', 'x' });
            graph.ReversePostOrder('f').ShouldBe(new char[] { 'j', 'f' });
            graph.ReversePostOrder('q').ShouldBe(new char[] { 'r', 'k', 'p', 'q' });
            graph.Validate().ShouldBe(true);
        }

        [Fact]
        public void ShouldDetectSimpleCycles()
        {
            var graph = new DirectedGraph<char>();
            graph.AddNodes('a', 'b');
            graph.AddEdge('a', 'b');
            graph.AddEdge('b', 'a');

            graph.IsCyclic('a').ShouldBe(true);
            graph.IsCyclic('b').ShouldBe(true);
            graph.Validate().ShouldBe(false);
            graph.Roots.Count().ShouldBe(0);

        }

        [Fact]
        public void ShoudlDetectDiamondCycles()
        {
            var graph = new DirectedGraph<char>();
            graph.AddNodes('a', 'b', 'c', 'd');
            graph.AddEdge('a', 'b');
            graph.AddEdge('b', 'c');
            graph.AddEdge('c', 'd');
            graph.AddEdge('d', 'a');


            graph.IsCyclic('a').ShouldBe(true);
            graph.IsCyclic('b').ShouldBe(true);
            graph.IsCyclic('c').ShouldBe(true);
            graph.IsCyclic('d').ShouldBe(true);
            graph.Validate().ShouldBe(false);
            graph.Roots.Count().ShouldBe(0);
        }

        [Fact]
        public void ShouldDetectComplexCycles()
        {
            //https://eli.thegreenplace.net/2015/directed-graph-traversal-orderings-and-applications-to-data-flow-analysis/
            var graph = new DirectedGraph<char>();
            graph.AddNodes('x', 't', 'c', 'b', 'e', 'd', 'r', 'p', 'j', 'f', 'k', 'q');
            graph.AddEdge('x', 't');
            graph.AddEdge('x', 'b');
            graph.AddEdge('x', 'c');

            graph.AddEdge('t', 'b');

            graph.AddEdge('b', 'd');

            graph.AddEdge('c', 'b');
            graph.AddEdge('c', 'e');

            graph.AddEdge('e', 'd');

            graph.AddEdge('f', 'j');

            graph.AddEdge('q', 'k');
            graph.AddEdge('q', 'p');

            graph.AddEdge('k', 'r');

            graph.AddEdge('p', 'r');


            //add cycle
            graph.AddEdge('c', 'f');
            graph.AddEdge('f', 'q');
            graph.AddEdge('r', 'c');

            graph.IsCyclic('x').ShouldBe(true);
            graph.Validate().ShouldBe(false);
        }

        [Fact]
        public void ShouldSupportMultipleRootsWithSharedChildren()
        {
            //https://eli.thegreenplace.net/2015/directed-graph-traversal-orderings-and-applications-to-data-flow-analysis/
            var graph = new DirectedGraph<char>();
            graph.AddNodes('x', 't', 'c', 'b', 'e', 'd', 'r', 'p', 'j', 'f', 'k', 'q');
            graph.AddEdge('x', 't');
            graph.AddEdge('x', 'b');
            graph.AddEdge('x', 'c');

            graph.AddEdge('t', 'b');

            graph.AddEdge('b', 'd');

            graph.AddEdge('c', 'b');
            graph.AddEdge('c', 'e');

            graph.AddEdge('e', 'd');

            graph.AddEdge('f', 'j');

            graph.AddEdge('q', 'k');
            graph.AddEdge('q', 'p');

            graph.AddEdge('k', 'r');

            graph.AddEdge('p', 'r');


            //add shared children
            graph.AddEdge('c', 'j');
            graph.AddEdge('k', 'j');

            graph.Roots.Count().ShouldBe(3);
            graph.ReversePostOrder().ShouldBe(new char[] { 'd', 'b', 't', 'e', 'j', 'c', 'x', 'f', 'r', 'k', 'p', 'q' });
            graph.Validate().ShouldBe(true);
        }

        [Fact]
        public void ShouldFailOnCyclicNodesWithValidRoot()
        {
            var graph = new DirectedGraph<char>();
            graph.AddNodes('a', 'b', 'c', 'd');
            graph.AddEdge('b', 'c');
            graph.AddEdge('c', 'd');
            graph.AddEdge('d', 'b');

            graph.Roots.Count().ShouldBe(1);
            graph.Validate().ShouldBe(false);
        }
    }
}
