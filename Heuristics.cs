using System;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace awkwardsimulator
{
    public abstract class Heuristic {
        abstract public float Score (GameState state, PlayerId pId);
    }

    public class WaypointHeuristic : Heuristic {
//        readonly GameState initialState;
        readonly PlayerId pId;

        readonly PlatformAStar pas;

        public WaypointHeuristic(GameState initialState, PlayerId pId) {
//            this.initialState = initialState;
            this.pId = pId;

            pas = new PlatformAStar (initialState.Platforms);
        }

        public Vector2 NextWaypoint(GameState state) {
            // Aim slightly above the next plaform
            return pas.NextPlatform (state.Player(pId), state.Goal).SurfaceCenter;
        }

        private int PlatformRank(GameState state, PlayerId pId) {
            return pas.PlatformPath (state.Player (pId), state.Goal).Count;
        }

        override public float Score(GameState state, PlayerId pId) {
            Player player = state.Player (pId);

            Vector2 nextWaypoint = NextWaypoint(state);
            int rank = PlatformRank (state, pId);

            var dist =
                Vector2.Distance (player.SurfaceCenter, nextWaypoint) +
                rank * (GameState.Width+GameState.Height);

            //            Debug.WriteLine("{0} {1}",
            //                Vector2.Distance (player.SurfaceCenter, nextWaypoint),
            //                Vector2.Distance (nextWaypoint, state.Goal.SurfaceCenter));

            return (float)dist;
        }

        private static double CosineSimilarity(Vector2 a, Vector2 b) {
            return Vector2.Dot (a, b) / (a.Length () * b.Length ());
        }

        private static bool SameDirection(Vector2 a, Vector2 b) {
            return CosineSimilarity (a, b) > 0;
        }

//        private static float fallHazard(Player player, Vector2 target) {
//            bool approachingNearest = SameDirection (player.Velocity, target - player.SurfaceCenter) &&
////                (player.RightBoundary < nearestPlat.LeftBoundary ||
////                    player.LeftBoundary > nearestPlat.RightBoundary);
//        }

    }

    public static class Heuristics
    {
        public static float linearHealthHeuristic(GameState state, PlayerId pId) {
            float goalDistance = Vector2.Distance(state.Player(pId).Coords, state.Goal.Coords);
            float healthScore  = System.Math.Abs(state.Health);

            var healthWeight = 0f;

            return goalDistance + (healthWeight * healthScore);
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

