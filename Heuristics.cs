using System;
using Microsoft.Xna.Framework;

namespace awkwardsimulator
{
    public static class Heuristics
    {
        public static float linearHealthHeuristic(this AI ai, GameState state) {
            return linearHealthHeuristic (state, ai.thisPlayer (state));
        }

        public static float linearHealthHeuristic(GameState state, Player player) {
            float goalDistance = Vector2.Distance(player.Coords, state.Goal.Coords);
            float healthScore  = System.Math.Abs(state.Health);

            var healthWeight = 0f;

            return goalDistance + (healthWeight * healthScore);
        }

        public static float SqrtDistance(GameState state, Vector2 a, Vector2 b) {
            var sqrtDist = (float)Math.Sqrt (Vector2.Distance (a, b));

            float healthScore  = System.Math.Abs(state.Health);

            var healthWeight = 1f;

            var composite = sqrtDist + (healthWeight * healthScore);

            return statusWrap(state, composite);
        }

        public static float heuristic(this AI ai, GameState state) {
            return heuristic (state, ai.thisPlayer (state));
        }

        public static float statusWrap(GameState state, float s) {
            var score = s;

            if (state.PlayStatus.isDied ()) {
                score = 99999;
            } else if (state.PlayStatus.isWon ()) {
                score = 0;
            }

            return score;
        }

        public static float heuristic(GameState state, Player p) {
            return statusWrap(state, linearHealthHeuristic(state, p));
        }
    }
}

