namespace Atma.Common
{
    using System.Collections.Generic;
    using System.Text;

    public sealed class DirectedGraph<T>
    {
        private HashSet<DirectedGraphNode> _nodes = new HashSet<DirectedGraphNode>();
        private Dictionary<T, DirectedGraphNode> _nodeLookup = new Dictionary<T, DirectedGraphNode>();
        private List<DirectedGraphNode> _order = new List<DirectedGraphNode>();

        //private List<DirectedGraphNode> _orphans;

        private T _cyclicNode;
        public T CyclicNode => _cyclicNode;

        private class DirectedGraphNode
        {
            public T Node;

            public bool Visited = false;
            public bool Recursion = false;

            public HashSet<DirectedGraphNode> Dependents = new HashSet<DirectedGraphNode>();
            public HashSet<DirectedGraphNode> Parents = new HashSet<DirectedGraphNode>();
        }

        public void AddNode(T node)
        {
            if (!_nodeLookup.TryGetValue(node, out var graphNode))
            {
                graphNode = new DirectedGraphNode() { Node = node };
                _nodeLookup.Add(node, graphNode);
                _nodes.Add(graphNode);
            }
        }

        public void AddNodes(params T[] nodes)
        {
            foreach (var it in nodes)
                AddNode(it);
        }

        public void Clear()
        {
            _nodes.Clear();
            _nodeLookup.Clear();
            _order.Clear();
        }

        public void AddEdge(T node, T dependent)
        {
            var graphNode = _nodeLookup[node];
            var dependentNode = _nodeLookup[dependent];

            dependentNode.Parents.Add(graphNode);
            graphNode.Dependents.Add(dependentNode);
        }

        public IEnumerable<T> Roots
        {
            get
            {
                foreach (var it in _nodes)
                    if (it.Parents.Count == 0)
                        yield return it.Node;
            }
        }

        private void ResetWalkData()
        {
            _order.Clear();
            foreach (var it in _nodes)
            {
                it.Visited = false;
                it.Recursion = false;
            }
        }

        public bool Validate()
        {

            _cyclicNode = default;

            var foundCyclicNode = false;
            foreach (var it in _nodes)
            {
                ResetWalkData();
                if (IsCyclic(it, out var cyclicNode))
                {
                    _cyclicNode = cyclicNode.Node;
                    foundCyclicNode = true;
                    break;
                }
            }

            return !foundCyclicNode;
        }

        public bool IsCyclic(T node)
        {
            ResetWalkData();
            return IsCyclic(_nodeLookup[node], out var _);
        }
        private bool IsCyclic(DirectedGraphNode node, out DirectedGraphNode cyclicNode)
        {
            if (!node.Visited)
            {
                node.Visited = true;
                node.Recursion = true;

                foreach (var it in node.Dependents)
                {
                    if (!it.Visited && IsCyclic(it, out cyclicNode))
                    {
                        node.Recursion = false;
                        return true;
                    }
                    else if (it.Recursion)
                    {
                        cyclicNode = node;
                        node.Recursion = false;
                        return true;
                    }
                }
            }

            cyclicNode = default;
            node.Recursion = false;
            return false;
        }

        private void Walk(DirectedGraphNode node)
        {
            if (!node.Visited && !node.Recursion)
            {
                node.Visited = true;
                node.Recursion = true;
                foreach (var it in node.Dependents)
                    Walk(it);

                node.Recursion = false;
                _order.Add(node);
            }
        }

        public IEnumerable<T> ReversePostOrder()
        {
            ResetWalkData();
            foreach (var it in Roots)
                Walk(_nodeLookup[it]);

            foreach (var it in _order)
                yield return it.Node;
        }

        public IEnumerable<T> ReversePostOrder(T node)
        {
            ResetWalkData();

            var rootNode = _nodeLookup[node];
            Walk(rootNode);

            foreach (var it in _order)
                yield return it.Node;
        }

        public IEnumerable<T> PostOrder()
        {
            ResetWalkData();
            foreach (var it in Roots)
                Walk(_nodeLookup[it]);

            for (var i = _order.Count - 1; i >= 0; i--)
                yield return _order[i].Node;
        }

        public IEnumerable<T> PostOrder(T node)
        {
            ResetWalkData();

            var rootNode = _nodeLookup[node];
            Walk(rootNode);

            for (var i = _order.Count - 1; i >= 0; i--)
                yield return _order[i].Node;
        }

        public override string ToString() => ToString(false);
        public string ToString(bool reverse)
        {
            var sb = new StringBuilder();

            foreach (var node in _nodeLookup)
            {
                var first = true;
                var items = reverse ? ReversePostOrder(node.Value.Node) : PostOrder(node.Value.Node);
                foreach (var walk in items)
                {
                    if (!first)
                        sb.Append("->");
                    first = false;

                    sb.Append(walk.ToString());
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

    }
}
