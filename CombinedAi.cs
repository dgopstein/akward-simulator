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

            return myPaths.Select(sn => Tuple.Create(sn.Key, sn.Value.ToPath())).ToList();
        }

        protected static double stateNodeScorer(CombinedHeuristic heuristic, StateNode node) {
            float h = (float)heuristic.Score (node.Value);

//            h += .1f * node.Depth(); // discourage long paths

            return h;
        }

        void addChildrenToOpenSet(TreeDictionary<double, StateNode> dict,
            StateNode parent, GameState state, CombinedHeuristic heuristic) {

            var parentScore = stateNodeScorer (heuristic, parent);
            foreach (var input in Input.All.CartesianProduct(Input.All)) {
                var stateNode = new StateNode (parent, input, state);

                var score = parentScore +
                    heuristic.EstimateScore (state, input.Item1, input.Item2,
                        state.Goal.Target, state.Goal.Target);

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
                    resultState = AStar.nextState(forwardModel, resultState, bestNextMove.Item1, bestNextMove.Item2);
                }

                addChildrenToOpenSet(openSet, best, resultState, heuristic);

                var stateNode = new StateNode (best, bestNextMove, resultState);
                var score = AStar.addNoise(stateNodeScorer (heuristic, stateNode));

                closedSet.Add(score, stateNode);

            } while(i++ < maxIters && !best.Value.PlayStatus.isWon() && openSet.Count > 0);

//            Debug.WriteLine ("closedSet size: {0}", closedSet.Count);
//            int k = 0;
//            foreach (var pth in closedSet) {
//                var pathStr = string.Join(", ", pth.Value.ToPath().Select(tup1 => tup1.Item1.Item1 + "|" + tup1.Item1.Item2));
//                Debug.WriteLine("closedSet[{0}]: {1} - {2}", k++, pth.Key, pathStr);
//            }

            lock (allPaths) { allPaths = closedSet; }

            var path = best.ToPath ();

            var deRooted = path.Count > 1 ? path.Skip (1) : path; // Ignore the root node

            var res = deRooted
                .SelectMany (t => Enumerable.Repeat(t.Item1,nRepetitions)).ToList();

            return res;
        }
    }
}

