using System;
using System.Linq;

using Microsoft.Xna.Framework;

using GobjPair = System.Tuple<awkwardsimulator.GameObject, awkwardsimulator.GameObject>;


namespace awkwardsimulator
{
    public class CombinedHeuristic {
        CombinedPlatformAStar cpas;

        public CombinedHeuristic(GameState state) {
            this.cpas = new CombinedPlatformAStar(state.Platforms);
        }

//        private float CombinedPlatformPathDistance(CombinedPlatformAStar cpas,
//            GameState state, GameObject target) {
//            var path = cpas.CombinedPlatformPath (state.P1, state.P2, state.Goal, state.Goal)
//                .SkipWhile(x => x.Item1 != target || x.Item2 != target).ToArray();
//
//            float dist = 0f;
//            for (int i = 0; i < path.Count () - 1; i++) {
//                dist += path [i].Item1.Distance (path [i + 1].Item1);
//                dist += path [i].Item1.Distance (path [i + 1].Item1);
//            }
//
//            return dist;
//        }

        public float CombinedDistance(CombinedPlatformAStar cpas,
            GameState state, GameObject next1, GameObject next2) {

            var combinedPath = cpas.CombinedPlatformPath (state.P1, state.P2, state.Goal, state.Goal);

            return Vector2.Distance (state.P1.SurfaceCenter, next1.Target) +
                   Vector2.Distance (state.P2.SurfaceCenter, next2.Target) +
                WaypointHeuristic.PlatformPathDistance(combinedPath.Select(x => x.Item1), next1) +
                WaypointHeuristic.PlatformPathDistance(combinedPath.Select(x => x.Item2), next2);
        }

        public float Score(GameState state) {
            Player p1 = state.P1;
            Player p2 = state.P2;

            GobjPair next = cpas.NextPlatform (p1, p2, state.Goal, state.Goal);

            var dist = CombinedDistance(cpas, state, next.Item1, next.Item2);
                
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
}

