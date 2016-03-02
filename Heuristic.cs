using System;
using Microsoft.Xna.Framework;

namespace awkwardsimulator
{
    public static class Heuristic
    {
        public static float linearHealthHeuristic(this AI ai, GameState state) {
            return linearHealthHeuristic (ai.thisPlayer (state), state);
        }

        public static float linearHealthHeuristic(Player player, GameState state) {
            float goalDistance = Vector2.Distance(player.Coords, state.Goal.Coords);
            float healthScore  = System.Math.Abs(state.Health);

            return goalDistance + 100 * healthScore;
        }

        public static float heuristic(this AI ai, GameState state) {
            return heuristic (ai.thisPlayer (state), state);
        }

        public static float heuristic(Player p, GameState state) {
            float score;

            if (state.PlayStatus ().isDied ()) {
                score = 99999;
            } else if (state.PlayStatus ().isWon ()) {
                score = 0;
            } else {
                score = linearHealthHeuristic(p, state);
            }

            return score;
        }
    }
}

