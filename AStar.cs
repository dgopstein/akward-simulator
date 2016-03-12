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

                double h = heuristic.Score (state);

                h =  Math.Truncate(h * 1000d) / 1000d; // remove fractional noise

                h += (0.0000001 * uniqueId++); // add marginal unique id to avoid collisions
                
//                h += 1 * s.Depth(); // discourage long paths

//                Debug.WritreeLine("[{0}] {1}: {2}", s.Depth(), s.Input, h);

                return h;
            };

            return scorer;
        }

//        static readonly List<Tuple<Input, Vector2>> moveVectors =
//            new List<Tuple<Input, Vector2>>(){
//                Tuple.Create(Input.Noop, new Vector2(0, 0)),
//                Tuple.Create(Input.Up, new Vector2(0, 1)),
//                Tuple.Create(Input.Right, new Vector2(1, 0)),
//                Tuple.Create(Input.UpRight, new Vector2(Math.Sqrt(2)/2, Math.Sqrt(2)/2)),
//                Tuple.Create(Input.Left, new Vector2(-1, 0)),
//                Tuple.Create(Input.UpLeft, new Vector2(-Math.Sqrt(2)/2, Math.Sqrt(2)/2))
//            };

        static readonly List<Input> wheelOfMoves = new List<Input>() {
            Input.Noop, Input.Left, Input.UpLeft, Input.Up, Input.UpRight, Input.Right
        };

        float estimateHeuristic(StateNode node, Input move) {
            // Favor similar moves
            int parentMoveIdx = wheelOfMoves.IndexOf (node.Input);
            int newMoveIdx = wheelOfMoves.IndexOf (move);

            float score = .2f * (wheelOfMoves.Count - (parentMoveIdx - newMoveIdx));

            if (score < 0) score *= -.8f;

            return score;
        }

        void addChildrenToOpenSet(SortedDictionary<double, StateNode> dict, StateNode parent, GameState state) {
            if (parent == null) {
                parent = new StateNode (parent, Input.Noop, state);
            }

            var parentScore = stateNodeScorer (parent, pId);
            foreach (var input in Input.All) {
                var stateNode = new StateNode (parent, input, state);
                dict.Add (parentScore + estimateHeuristic(parent, input), stateNode);
            }
        }

        protected StateNodeScorer stateNodeScorer;

        override public List<Input> nextInputs (GameState origState, PlayerId pId, Heuristic heuristic)
        {
            var openSet = new SortedDictionary<double, StateNode>(); // Known, but unexplored
            var closedSet = new SortedDictionary<double, StateNode>(); // Fully explored

            stateNodeScorer = scorerGenerator (heuristic);

//            var root = new StateNode (null, new Input(), origState);
//            openSet.Add(stateNodeScorer(root, pId), root);
            addChildrenToOpenSet(openSet, null, origState);

            int maxIters = 50;
            int nRepetitions = 4;

            StateNode best;

            int i = 0;
            do {
                var bestKV = openSet.First ();
                openSet.Remove(bestKV.Key);

                best = bestKV.Value;

                if (pId == PlayerId.P2)
                    Debug.WriteLine("{0}: {1} {2}", bestKV.Key, best.Input, best.Value.P2.Coords);

                var bestNextMove = best.Input;

                // repeat the same input a few times
                GameState resultState = best.Value;
                for (int j = 0; j < nRepetitions; j++) {
                    resultState = nextState(resultState, bestNextMove);
                }

                addChildrenToOpenSet(openSet, best, resultState);

                var stateNode = new StateNode (best, bestNextMove, resultState);
                var score = stateNodeScorer (stateNode, pId);

                closedSet.Add(score, stateNode);

            } while(i++ < maxIters && !best.Value.PlayStatus.isWon() && openSet.Count > 0);

            lock (allPaths) { allPaths = closedSet; }

            var path = best.ToPath ();

            var deRooted = path.Count > 1 ? path.Skip (1) : path; // Ignore the root node

            var res = deRooted
                .SelectMany (t => Enumerable.Repeat(t.Item1,nRepetitions)).ToList();

//            Debug.WriteLine ("res " + string.Join(" ", res.Select(x => x.shortString())));

            return res;
        }

        override protected Input predictPartnerInput(GameState state) {
            return Input.UpRight;
//            return nextInputs (state, this.otherPlayer(state), heuristic).First();
        }
    }
}
