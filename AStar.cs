using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;


using StateNode = awkwardsimulator.AStarNode<awkwardsimulator.GameState, awkwardsimulator.Input>;
using StateNodeScorer = System.Func<awkwardsimulator.AStarNode<awkwardsimulator.GameState, awkwardsimulator.Input>, awkwardsimulator.PlayerId, double>;
//using Heuristic = System.Func<awkwardsimulator.GameState, awkwardsimulator.PlayerId, float>;
using SD.Tools.Algorithmia.GeneralDataStructures;
using MoreLinq;
using Microsoft.Xna.Framework;
using C5;

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

        override public string ToString() {
            var prefix = (parent != null) ? parent.ToString() : "";

            return prefix + " " + Value.ToString();
        }
    }

    public class AStar : AI
    {
        protected TreeDictionary<double, StateNode> allPaths;

        public AStar (GameState state, PlayerId pId, Heuristic heuristic) : base(state, pId, heuristic) {
            allPaths = new TreeDictionary<double, StateNode> ();
        }

        override public List<Tuple<double, List<Tuple<Input, GameState>>>> BestPaths(int n) {
            List<C5.KeyValuePair<double,StateNode>> myPaths;
            lock(allPaths) {
                myPaths = allPaths.Take (n).ToList();
            }

            return myPaths.Select(sn => Tuple.Create(sn.Key, sn.Value.ToPath())).ToList();
        }

        override public List<Tuple<double, List<Tuple<Input, GameState>>>> AllPaths() {
            List<C5.KeyValuePair<double,StateNode>> myPaths;
            lock(allPaths) {
                myPaths = allPaths.ToList();
            }

            return myPaths.Select(sn => Tuple.Create(sn.Key, sn.Value.ToPath())).ToList();
        }

        static int uniqueId = 1;
        const int noisePrecision = 100000;
        public static double addNoise(double f) {
            double ret = Math.Truncate(f * noisePrecision) / noisePrecision; // remove fractional noise
            ret += (0.0001 * (1.0 / noisePrecision)) * (uniqueId++ % noisePrecision); // add marginal unique id to avoid collisions
            return ret;
        }

        protected static double stateNodeScorer(Heuristic heuristic, StateNode node) {
            float h = (float)heuristic.Score (node.Value);

            h += .1f * node.Depth(); // discourage long paths

            return h;
        }
 

//        Profiler prof = new Profiler ("addChildren", 3000, 3);
        void addChildrenToOpenSet(TreeDictionary<double, StateNode> dict,
                                  StateNode parent, GameState state, Heuristic heuristic) {
//            prof.Start ();

            var parentScore = stateNodeScorer (heuristic, parent);
            foreach (var input in Input.All) {
                var stateNode = new StateNode (parent, input, state);

                var score = parentScore + heuristic.EstimateScore (state, input);

                var noiseyScore = addNoise (score);
                dict.Add (noiseyScore, stateNode);
            }

//            prof.End ();
        }

//        protected StateNodeScorer stateNodeScorer;

        override public List<Input> nextInputList (GameState origState, PlayerId pId, Heuristic heuristic)
        {
            var openSet = new TreeDictionary<double, StateNode>(); // Known, but unexplored
            var closedSet = new TreeDictionary<double, StateNode>(); // Fully explored

//            stateNodeScorer = scorerGenerator (heuristic);

            StateNode parentStub = new StateNode (null, Input.Noop, origState);
            addChildrenToOpenSet(openSet, parentStub, origState, heuristic);

            int maxIters = 30;
            int nRepetitions = 3;

            StateNode best;

            int i = 0;
            do {
                var bestKV = openSet.First ();
                openSet.Remove(bestKV.Key);

                best = bestKV.Value;

                var bestNextMove = best.Input;

                // repeat the same input a few times
                GameState resultState = best.Value;
                for (int j = 0; j < nRepetitions; j++) {
                    resultState = nextState(resultState, bestNextMove);
                }

                addChildrenToOpenSet(openSet, best, resultState, heuristic);

                var stateNode = new StateNode (best, bestNextMove, resultState);
                var score = stateNodeScorer (heuristic, stateNode);

                closedSet.Add(score, stateNode);

            } while(i++ < maxIters && !best.Value.PlayStatus.isWon() && openSet.Count > 0);

            lock (allPaths) { allPaths = closedSet; }

            var path = best.ToPath ();

            var deRooted = path.Count > 1 ? path.Skip (1) : path; // Ignore the root node

            var res = deRooted
                .SelectMany (t => Enumerable.Repeat(t.Item1,nRepetitions)).ToList();

            return res;
        }

        override protected Input predictPartnerInput(GameState state) {
            return Input.UpRight;
        }
    }
}
