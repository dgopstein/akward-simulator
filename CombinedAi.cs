using System;

using StateNode = awkwardsimulator.AStarNode<awkwardsimulator.GameState, System.Tuple<awkwardsimulator.Input, awkwardsimulator.Input>>;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using C5;
using System.Diagnostics;

namespace awkwardsimulator
{
    public class CombinedAi
    {
        protected TreeDictionary<double, StateNode> allPaths;
        ForwardModel forwardModel;

        CombinedHeuristic heuristic;
        public CombinedHeuristic Heuristic { get { return heuristic; } }

        public CombinedAi (GameState state) {
            this.heuristic = new CombinedHeuristic (state);
            this.forwardModel = new ForwardModel(state);
            this.allPaths = new TreeDictionary<double, StateNode> ();
        }

        public List<Tuple<double, List<Tuple<Tuple<Input, Input>, GameState>>>> AllPaths() {
            List<C5.KeyValuePair<double,StateNode>> myPaths;
            lock(allPaths) {
                myPaths = allPaths.ToList();
            }

            return myPaths.Select(sn => Tuple.Create(sn.Key, sn.Value.ToPath().Skip(1).ToList())).ToList();
        }

        protected static double pathScore(StateNode node) {
            if (node.Parent == null) {
                return 0;
            } else {
                return Vector2.Distance (node.Value.P1.Center, node.Parent.Value.P1.Center) +
                       Vector2.Distance (node.Value.P2.Center, node.Parent.Value.P2.Center) +
                       pathScore(node.Parent);
            }
        }

        protected static double stateNodeScorer(CombinedHeuristic heuristic, StateNode node) {
            double h = heuristic.Score (node.Value);

            h += pathScore (node);

//            h += .1f * node.Depth(); // discourage long paths

            return (float)h;
        }

        void addChildrenToOpenSet(TreeDictionary<double, StateNode> dict,
            StateNode parent, GameState state, CombinedHeuristic heuristic) {

            var parentScore = stateNodeScorer (heuristic, parent);
            foreach (var input in Input.All.CartesianProduct(Input.All)) {
                var stateNode = new StateNode (parent, input, state);

//                var target1 = state.Goal;
//                var target2 = state.Goal;

                var targets = heuristic.cpas.NextPlatform (state.P1, state.P2, state.Goal, state.Goal);
                var target1 = targets.Item1;
                var target2 = targets.Item2;

                var score = parentScore +
                    heuristic.EstimateScore (state, input.Item1, input.Item2,
                        target1.Target, target2.Target);

                var noiseyScore = AStar.addNoise (score);
                dict.Add (noiseyScore, stateNode);
            }
        }

//        public TreeDictionary<double, IEnumerable<B>> AStar<A, B>(Func<B, AStarNode<A, B>> nextState, A state) {
//            var openSet = new TreeDictionary<double, B>(); // Known, but unexplored
//            var closedSet = new TreeDictionary<double, B>(); // Fully explored
//
//            AStarNode<A, B> parentStub = new AStarNode<A, B> (null, null, state);
//            addChildrenToOpenSet(openSet, parentStub, state, heuristic);
//
//            int maxIters = 100;
//            int nRepetitions = 5;
//
//            AStarNode<A, B> best;
//
//            int i = 0;
//            do {
//                var bestKV = openSet.First ();
//                openSet.Remove(bestKV.Key);
//
//                best = bestKV.Value;
//
//                var bestNextMove = best.Input;
//
//                // repeat the same input a few times
//                B resultState = best.Value;
//                for (int j = 0; j < nRepetitions; j++) {
//                    resultState = nextState(new AStarNode<A, B>(best, bestNextMove, resultState));
//                }
//
//                addChildrenToOpenSet(openSet, best, resultState, heuristic);
//
//                var stateNode = new AStarNode<A, B> (best, bestNextMove, resultState);
//                var score = AStar.addNoise(stateNodeScorer (heuristic, stateNode));
//
//                closedSet.Add(score, stateNode);
//
//            } while(i++ < maxIters && closedSet.First().Key > 0 && openSet.Count > 0);
//
//            return closedSet;
//        }
//
//        public List<Tuple<Input, Input>> nextInputsList2(GameState state) {
//        
//        }

        public List<Tuple<Input, Input>> nextInputsList(GameState state) {
            var openSet = new TreeDictionary<double, StateNode>(); // Known, but unexplored
            var closedSet = new TreeDictionary<double, StateNode>(); // Fully explored

            StateNode parentStub = new StateNode (null, Tuple.Create(Input.Noop, Input.Noop), state);
            addChildrenToOpenSet(openSet, parentStub, state, heuristic);

            int maxIters = 100;
            int nRepetitions = 5;

            StateNode bestOpen;

            int i = 0;
            do {
                var bestOpenKV = openSet.First ();
                openSet.Remove(bestOpenKV.Key);

                bestOpen = bestOpenKV.Value;

                var bestNextMove = bestOpen.Input;

                // repeat the same input a few times
                GameState resultState = bestOpen.Value;
                for (int j = 0; j < nRepetitions; j++) {
                    resultState = AStar.nextState(forwardModel, resultState, bestNextMove.Item1, bestNextMove.Item2);
                }

                addChildrenToOpenSet(openSet, bestOpen, resultState, heuristic);

                var stateNode = new StateNode (bestOpen, bestNextMove, resultState);
                var score = AStar.addNoise(stateNodeScorer (heuristic, stateNode));

                closedSet.Add(score, stateNode);

            } while(i++ < maxIters && !closedSet.First().Value.Value.PlayStatus.isWon() && openSet.Count > 0);

//            Debug.WriteLine ("closedSet size: {0}", closedSet.Count);
//            int k = 0;
//            foreach (var pth in closedSet) {
//                var pathStr = string.Join(", ", pth.Value.ToPath().Select(tup1 => tup1.Item1.Item1 + "|" + tup1.Item1.Item2));
//                Debug.WriteLine("closedSet[{0}]: {1} - {2}", k++, pth.Key, pathStr);
//            }

            lock (allPaths) { allPaths = closedSet; }

            var path = closedSet.First().Value.ToPath ();

            var deRooted = path.Count > 1 ? path.Skip (1) : path; // Ignore the root node


            Debug.Print("bestPath1: {0}", moveListStr(deRooted.Select(x => x.Item1.Item1)));
            Debug.Print("bestPath2: {0}", moveListStr(deRooted.Select(x => x.Item1.Item2)));

            var res = deRooted
                .SelectMany (t => Enumerable.Repeat(t.Item1,nRepetitions)).ToList();

            return res;
        }

        public static string moveListStr(IEnumerable<Input> moves) {
            return string.Join ("", moves.Select (i => i.shortString()));
        }
    }
}

