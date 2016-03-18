using System;

using StateNode = awkwardsimulator.AStarNode<awkwardsimulator.GameState, System.Tuple<awkwardsimulator.Input, awkwardsimulator.Input>>;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;


namespace awkwardsimulator
{
    public class CombinedHeuristic {
        PlatformAStar pas1;
        PlatformAStar pas2;

        public CombinedHeuristic(GameState state) {
            this.pas1 = new PlatformAStar(state.Platforms);
            this.pas2 = new PlatformAStar(state.Platforms);
        }

        public float Score(GameState state) {
            Player p1 = state.P1;
            Player p2 = state.P2;

            GameObject next1 = pas1.NextPlatform (p1, state.Goal);
            GameObject next2 = pas2.NextPlatform (p2, state.Goal);

            var dist =
                WaypointHeuristic.PlayerDistance (pas1, state, PlayerId.P1, next1) +
                WaypointHeuristic.PlayerDistance (pas2, state, PlayerId.P2, next2);

            return statusWrap(state, dist);
        }

        public float EstimateScore(GameState state, Input move1, Input move2,
                                   Vector2 target1, Vector2 target2) {
            return WaypointHeuristic.EstimateScore (PlayerId.P1, state, move1, target1) +
                   WaypointHeuristic.EstimateScore (PlayerId.P2, state, move2, target2);

        }

        public static float statusWrap(GameState state, float s) {
            float healthScore  = System.Math.Abs(state.Health);

            var healthWeight = 150f;

            var score = s  + (healthWeight * healthScore);

            float minPlatHeight = state.Platforms.Min (x => x.TopBoundary);

            if (state.PlayStatus.isDied () ||
                state.P1.BottomBoundary < minPlatHeight ||
                state.P2.BottomBoundary < minPlatHeight) {
                score = 5*GameState.Width;
            } else if (state.PlayStatus.isWon ()) {
                score = 0;
            }

            return score;
        }
    }

    public class CombinedAi
    {
        protected SortedDictionary<double, StateNode> allPaths;
        CombinedHeuristic heuristic;
        ForwardModel forwardModel;

        public CombinedAi (GameState state) {
            this.heuristic = new CombinedHeuristic (state);
            this.forwardModel = new ForwardModel(state);
            this.allPaths = new SortedDictionary<double, StateNode> ();
        }

        public List<Tuple<double, List<Tuple<Tuple<Input, Input>, GameState>>>> AllPaths() {
            List<KeyValuePair<double,StateNode>> myPaths;
            lock(allPaths) {
                myPaths = allPaths.ToList();
            }

            return myPaths.Select(sn => Tuple.Create(sn.Key, sn.Value.ToPath())).ToList();
        }

        protected static double stateNodeScorer(CombinedHeuristic heuristic, StateNode node) {
            float h = (float)heuristic.Score (node.Value);

            h += .1f * node.Depth(); // discourage long paths

            return h;
        }

        void addChildrenToOpenSet(SortedDictionary<double, StateNode> dict,
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

        public List<Tuple<Input, Input>> nextInputsList(GameState state) {
            var openSet = new SortedDictionary<double, StateNode>(); // Known, but unexplored
            var closedSet = new SortedDictionary<double, StateNode>(); // Fully explored

            //            stateNodeScorer = scorerGenerator (heuristic);

            StateNode parentStub = new StateNode (null, Tuple.Create(Input.Noop, Input.Noop), state);
            addChildrenToOpenSet(openSet, parentStub, state, heuristic);

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
                    resultState = AStar.nextState(forwardModel, resultState, bestNextMove.Item1, bestNextMove.Item2);
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
    }
}

