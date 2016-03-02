using System;
using Microsoft.Xna.Framework;

namespace awkwardsimulator
{
    public static class Heuristic
    {
        public static float linearHealthHeuristic(this AI ai, GameState state) {
            return linearHealthHeuristic (ai.thisPlayer (state), state.Goal, state.Health);
        }

        public static float linearHealthHeuristic(Player player, Goal goal, float health) {
            float goalDistance = Vector2.Distance(player.Coords, goal.Coords);
            float healthScore  = System.Math.Abs(health);
            
            return goalDistance + healthScore;
        }

        public static float heuristic(this AI ai, GameState state) {
            return linearHealthHeuristic(ai, state);
        }
    }
}

