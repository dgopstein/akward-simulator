using System;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace awkwardsimulator
{
    public abstract class Heuristic {
        abstract public float Score (GameState state, PlayerId pId);

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

    public class WaypointHeuristic : Heuristic {
//        readonly GameState initialState;
        readonly PlayerId pId;

        readonly PlatformAStar pas;

        public WaypointHeuristic(GameState initialState, PlayerId pId) {
//            this.initialState = initialState;
            this.pId = pId;

            pas = new PlatformAStar (initialState.Platforms);
        }

        public GameObject NextPlatform(GameState state) {
            return pas.NextPlatform (state.Player(pId), state.Goal);
        }

        private int PlatformRank(GameState state, PlayerId pId, GameObject platform) {
            var path = pas.PlatformPath (state.Player (pId), state.Goal);
            return path.Count - path.IndexOf (platform);
        }

        override public float Score(GameState state, PlayerId pId) {
            Player player = state.Player (pId);

            GameObject next = NextPlatform (state);
            int rank = PlatformRank (state, pId, next);

            var dist =
                Vector2.Distance (player.SurfaceCenter, next.SurfaceCenter) +
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

    public class LinearHeuristic : Heuristic {
        public LinearHeuristic() {}

        override public float Score(GameState state, PlayerId pId) {
            float goalDistance = Vector2.Distance(state.Player(pId).Coords, state.Goal.Coords);
            float healthScore  = System.Math.Abs(state.Health);

            var healthWeight = 0f;

            return goalDistance + (healthWeight * healthScore);
        }
    }
}

