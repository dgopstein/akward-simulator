using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;


using StateNode = awkwardsimulator.AStarNode<awkwardsimulator.GameState, awkwardsimulator.Input>;

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
        virtual protected int maxIters() {return 50;}
    
        protected SortedDictionary<double, StateNode> allPaths;

        public AStar (GameState state, PlayerId pId) : base(state, pId) {
            allPaths = new SortedDictionary<double, StateNode> ();
        }

        override public List<List<Tuple<Input, GameState>>> BestPaths() {
            List<KeyValuePair<double,StateNode>> myPaths;
            lock(allPaths) {
                myPaths = allPaths.Take (5).ToList();
            }

            return myPaths.Select(sn => sn.Value.ToPath()).ToList();
        }

        protected virtual Func<StateNode, double>  heuristicGenerator() {
            int uniqueId = 1;
            Func<StateNode, double> heuristic = (s) => {
                double h = Heuristics.heuristic (this.thisPlayer (s.Value), s.Value);

                h =  Math.Truncate(h * 1000d) / 1000d; // remove fractional noise
                
                h += (0.0000001 * uniqueId++); // add marginal unique id to avoid collisions
                
                h += 1 * s.Depth(); // discourage long paths

                return h;
            };

            return heuristic;
        }

        protected Func<StateNode, double> heuristic;
        override public float Heuristic(GameState state) {
            if (heuristic == null) heuristic = heuristicGenerator ();

            return (float)heuristic(new StateNode(null, new Input(), state));
        }

        public override Input nextInput (GameState origState)
        {
            var paths = new SortedDictionary<double, StateNode>();

            heuristic = heuristicGenerator ();

            var root = new StateNode (null, new Input(), origState);
            paths.Add(heuristic(root), root);
            var best = root;

            for (int i = 0; i < maxIters() && !best.Value.PlayStatus.isWon() && paths.Count > 0; i++) {
                best.Children = Input.All.ToDictionary (input=>input,
                    input => {

                        // repeat the same input a few times
                        GameState s = best.Value;
                        for (int j = 0; j < 3; j++) {
                            s = nextState(s, input);
                        }

                        return new StateNode(best, input, s);
                    });

                foreach (var c in best.Children) {
                    paths.Add(heuristic (c.Value), c.Value);
                }



                best = paths.First().Value;

//                Debug.WriteLine ("{0}: [{1}]", paths.Count,
//                    string.Join(", ", paths.Select(p => p.Value.Value.P1.Coords)));
                paths.Remove (paths.First().Key);
            }

            lock (allPaths) { allPaths = paths; }

            var res = best.FirstAncestor ().Input;
            return res;
        }
    }
}
