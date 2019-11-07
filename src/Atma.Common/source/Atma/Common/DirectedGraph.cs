namespace Atma.Common
{
    using System.Collections.Generic;

    public sealed class DirectedGraph<T>
    {
        private HashSet<DirectedGraphNode> _nodes = new HashSet<DirectedGraphNode>();
        private Dictionary<T, DirectedGraphNode> _nodeLookup = new Dictionary<T, DirectedGraphNode>();
        private List<DirectedGraphNode> _order = new List<DirectedGraphNode>();

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

        public bool Validate(bool throwOnError = false)
        {
            var hasRoots = false;
            foreach (var it in Roots)
            {
                hasRoots = true;
                if (IsCyclic(it, throwOnError))
                    return false;
            }

            if (throwOnError && !hasRoots)
                throw new System.Exception("The graph has no roots.");

            return hasRoots;
        }

        public bool IsCyclic(T node, bool throwOnError = false)
        {
            ResetWalkData();
            return IsCyclic(_nodeLookup[node], throwOnError);
        }

        private bool IsCyclic(DirectedGraphNode node, bool throwOnError)
        {
            if (!node.Visited)
            {
                node.Visited = true;
                node.Recursion = true;

                foreach (var it in node.Dependents)
                {
                    if (!it.Visited && IsCyclic(it, throwOnError))
                    {
                        if (throwOnError)
                            throw new System.Exception("Cyclic dependency on " + it.Node.ToString());
                        return true;
                    }
                    else if (it.Recursion)
                    {
                        if (throwOnError)
                            throw new System.Exception("Cyclic dependency on " + it.Node.ToString());
                        return true;
                    }
                }
            }

            node.Recursion = false;
            return false;
        }

        private void Walk(DirectedGraphNode node)
        {
            node.Visited = true;
            foreach (var it in node.Dependents)
                if (!it.Visited)
                    Walk(it);

            _order.Add(node);
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

    }
}
