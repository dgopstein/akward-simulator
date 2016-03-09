using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;


using StateNode = awkwardsimulator.AStarNode<awkwardsimulator.GameState, awkwardsimulator.Input>;
using StateNodeScorer = System.Func<awkwardsimulator.AStarNode<awkwardsimulator.GameState, awkwardsimulator.Input>, awkwardsimulator.PlayerId, double>;
//using Heuristic = System.Func<awkwardsimulator.GameState, awkwardsimulator.PlayerId, float>;



namespace awkwardsimulator
{
    public class AStarNode<P, T> {
        private AStarNode<P, T> parent;
        public  AStarNode<P, T> Parent { get { return parent; } }

        private T input;
        public  T Input { get { return input; } }

        private P state;
        public  P Value { get { return state; } }

        private Dictionary<T, AStarNode<P, T>> children;
        public  Dictionary<T, AStarNode<P, T>> Children {
            get { return children;  }
            set { children = value; }
        }
            
        public AStarNode(AStarNode<P, T> parent, T input, P state) {
            this.parent = parent;
            this.input = input;
            this.state = state;

            this.children = new Dictionary<T, AStarNode<P, T>> ();
        }

        public AStarNode<P, T> FirstAncestor () {
            AStarNode<P, T> node;

            if (parent == null) {
                Debug.WriteLine ("Returning the root!!!");
                node = this;
            } else if (parent.parent == null) {
                node = this;
            } else {
                node = parent.FirstAncestor ();
            }

            return node;
        }

        private int depth = -1;
        public int Depth () {
            if (depth < 0) {
                depth = (parent == null) ? 0 : parent.Depth() + 1;
            }

            return depth;
        }

        public List<Tuple<T, P>> ToPath() {
            Stack<Tuple<T, P>> stack = new Stack<Tuple<T, P>> ();

            AStarNode<P, T> node = this;
            do {
                stack.Push (Tuple.Create (node.Input, node.Value));
                node = node.Parent;
            } while (node != null);

            return stack.ToList ();
        }
    }

    public class AStar : AI
    {
        protected SortedDictionary<double, StateNode> allPaths;

        public AStar (GameState state, PlayerId pId, Heuristic heuristic) : base(state, pId, heuristic) {
            allPaths = new SortedDictionary<double, StateNode> ();
        }

        override public List<Tuple<double, List<Tuple<Input, GameState>>>> BestPaths(int n) {
            List<KeyValuePair<double,StateNode>> myPaths;
            lock(allPaths) {
                myPaths = allPaths.Take (n).ToList();
            }

            return myPaths.Select(sn => Tuple.Create(sn.Key, sn.Value.ToPath())).ToList();
        }

        override public List<Tuple<double, List<Tuple<Input, GameState>>>> AllPaths() {
            List<KeyValuePair<double,StateNode>> myPaths;
            lock(allPaths) {
                myPaths = allPaths.ToList();
            }

            return myPaths.Select(sn => Tuple.Create(sn.Key, sn.Value.ToPath())).ToList();
        }

        protected virtual StateNodeScorer scorerGenerator(Heuristic heuristic) {
            int uniqueId = 1;
            StateNodeScorer scorer = (s, pId) => {
                var state = s.Value;

                double h = heuristic.Score (state, pId);

                h =  Math.Truncate(h * 1000d) / 1000d; // remove fractional noise

                h += (0.0000001 * uniqueId++); // add marginal unique id to avoid collisions
                
//                h += 1 * s.Depth(); // discourage long paths

//                Debug.WritreeLine("[{0}] {1}: {2}", s.Depth(), s.Input, h);

                return h;
            };

            return scorer;
        }

        protected StateNodeScorer stateNodeScorer;

        override public List<Input> nextInputs (GameState origState, PlayerId pId, Heuristic heuristic)
        {
            var paths = new SortedDictionary<double, StateNode>();


            stateNodeScorer = scorerGenerator (heuristic);

            var root = new StateNode (null, new Input(), origState);
            paths.Add(stateNodeScorer(root, pId), root);
            var best = root;

            int maxIters = 10;
            int nRepetitions = 3;

            for (int i = 0; i < maxIters && !best.Value.PlayStatus.isWon() && paths.Count > 0; i++) {
                best.Children = Input.All.ToDictionary (input=>input,
                    input => {
                        // repeat the same input a few times
                        GameState s = best.Value;
                        for (int j = 0; j < nRepetitions; j++) {
                            s = nextState(s, input);
                        }

                        return new StateNode(best, input, s);
                    });

                foreach (var c in best.Children) {
                    paths.Add(stateNodeScorer (c.Value, pId), c.Value);
                }
                best = paths.First().Value;

                paths.Remove (paths.First().Key);
            }

            lock (allPaths) { allPaths = paths; }

            var res = best.ToPath ().SelectMany (t => Enumerable.Repeat(t.Item1,nRepetitions)).ToList();
            return res;
        }

        override protected Input predictPartnerInput(GameState state) {
            return Input.UpRight;
//            return nextInputs (state, this.otherPlayer(state), heuristic).First();
        }
    }
}
