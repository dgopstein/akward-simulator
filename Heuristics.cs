using System;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Linq;

namespace awkwardsimulator
{
    public abstract class Heuristic {
        protected readonly PlayerId pId;

        public Heuristic(PlayerId pId) {
            this.pId = pId;
        }

        abstract public float Score (GameState state);

        protected float statusWrap(GameState state, float s) {
            float healthScore  = System.Math.Abs(state.Health);

            var healthWeight = 0f;

            var score = s  + (healthWeight * healthScore);

            if (//state.PlayStatus.isDied ()){// ||
                state.Player(pId).Bottom < state.Platforms.Min(x => x.Top)) {
                score = GameState.Width;
            } else if (state.PlayStatus.isWon ()) {
                score = 0;
            }

            return score;
        }
    }

    public class WaypointHeuristic : Heuristic {
//        readonly GameState initialState;
        readonly PlatformAStar pas;

        public WaypointHeuristic(GameState initialState, PlayerId pId) : base(pId) {
//            this.initialState = initialState;
            pas = new PlatformAStar (initialState.Platforms);
        }

        public GameObject NextPlatform(GameState state) {
            return pas.NextPlatform (state.Player(pId), state.Goal);
        }

        private float PlatformPathDistance(GameState state, PlayerId pId, GameObject platform) {
            var path = pas.PlatformPath (state.Player (pId), state.Goal).SkipWhile(x => x != platform).ToArray();

            float dist = 0f;
            for (int i = 0; i < path.Count () - 1; i++) {
                dist += path [i].Distance (path [i + 1]);
            }

            return dist;
        }

        override public float Score(GameState state) {
            Player player = state.Player (pId);

            GameObject next = NextPlatform (state);

            var dist =
                Vector2.Distance (player.SurfaceCenter, next.SurfaceCenter) +
                fallHazard(player, next.SurfaceCenter) +
                PlatformPathDistance (state, pId, next);
            
            //            Debug.WriteLine("{0} {1}",
            //                Vector2.Distance (player.SurfaceCenter, nextWaypoint),
            //                Vector2.Distance (nextWaypoint, state.Goal.SurfaceCenter));

            return statusWrap(state, dist);
//            return dist;
        }

        private static double CosineSimilarity(Vector2 a, Vector2 b) {
            return Vector2.Dot (a, b) / (a.Length () * b.Length ());
        }

        private static bool SameDirection(Vector2 a, Vector2 b) {
            return CosineSimilarity (a, b) > 0;
        }

        private static float fallHazard(Player player, Vector2 target) {
            if (player.Y <= target.Y && player.Velocity.Y < 0) {
                return 10;
            } else {
                return 0;
            }
        }

    }

    public class LinearHeuristic : Heuristic {
        public LinearHeuristic(PlayerId pId) : base(pId)  {}

        override public float Score(GameState state) {
            float goalDistance = Vector2.Distance(state.Player(pId).Coords, state.Goal.Coords);
            float healthScore  = System.Math.Abs(state.Health);

            var healthWeight = 0f;

            return goalDistance + (healthWeight * healthScore);
        }
    }
}

