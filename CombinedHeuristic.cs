using System;
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
}

