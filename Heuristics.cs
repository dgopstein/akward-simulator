using System;
using Microsoft.Xna.Framework;
using Heuristic = System.Func<awkwardsimulator.GameState, awkwardsimulator.Player, float>;
using System.Diagnostics;

namespace awkwardsimulator
{
    public static class Heuristics
    {
        public static float linearHealthHeuristic(GameState state, PlayerId pId) {
            float goalDistance = Vector2.Distance(state.Player(pId).Coords, state.Goal.Coords);
            float healthScore  = System.Math.Abs(state.Health);

            var healthWeight = 0f;

            return goalDistance + (healthWeight * healthScore);
        }

        static PlatformAStar pas;
        static public Vector2 NextWaypoint(GameState state, PlayerId pId) {
            if (pas == null) pas = new PlatformAStar (state.Platforms);
            return pas.NextPlatform (state.Player(pId), state.Goal).SurfaceCenter;
        }

//        public static float SqrtDistance(GameState state, Vector2 a, Vector2 b) {
        public static float WaypointDistance(GameState state, PlayerId pId) {
            Player player = state.Player (pId);

            Vector2 nextWaypoint = NextWaypoint(state, pId);

            var dist =
                Vector2.Distance (player.SurfaceCenter, nextWaypoint) +
                Vector2.Distance (nextWaypoint, state.Goal.SurfaceCenter);


            return (float)dist;
        }

        public static float statusWrap(GameState state, float s) {
            float healthScore  = System.Math.Abs(state.Health);

            var healthWeight = 0f;

            var score = s  + (healthWeight * healthScore);

            if (state.PlayStatus.isDied ()) {
                score = 99999;
            } else if (state.PlayStatus.isWon ()) {
                score = 0;
            }

            return score;
        }
    }
}

