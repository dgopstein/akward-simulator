using System;
using Microsoft.Xna.Framework;

namespace awkwardsimulator
{
    public static class Heuristic
    {
        public static float linearHealthHeuristic(this AI ai, GameState state) {
            Player thisPlayer = ai.thisPlayer (state);

            Vector2 position = thisPlayer.Coords;
            float goal = Vector2.Distance(position, state.Goal.Coords);

            float minHealth = 0.5f;
            float health =  System.Math.Abs(state.Health - minHealth);
            health  = goal * health;

            //Console.WriteLine(health);

            return goal + health;
        }

        public static float heuristic(this AI ai, GameState state) {
            return linearHealthHeuristic(ai, state);
        }
    }
}

