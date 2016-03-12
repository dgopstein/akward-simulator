using System;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

namespace awkwardsimulator
{
    public abstract class Heuristic {
        protected readonly PlayerId pId;

        public Heuristic(PlayerId pId) {
            this.pId = pId;
        }

        abstract public float Score (GameState state);
        abstract public float EstimateScore (GameState state, Input move);

        private static readonly float diagonangle = (float)Math.Sqrt(2)/2;
        private static readonly Dictionary<Input, Vector2> moveVectors =
            new Dictionary<Input, Vector2>(){
            { Input.Noop, new Vector2(0, 0) },
            { Input.Up, new Vector2(0, 1) },
            { Input.Right, new Vector2(1, 0) },
            { Input.UpRight, new Vector2(diagonangle, diagonangle) },
            { Input.Left, new Vector2(-1, 0) },
            { Input.UpLeft, new Vector2(-diagonangle, diagonangle) },
        };

        protected float EstimateScore(GameState state, Input move, Vector2 target) {
            if (move == Input.Noop)
                return 0f;

            Vector2 targetVector = Vector2.Subtract (target, state.Player (pId).Target);

            float similarity = (float)Util.CosineSimilarity(targetVector, moveVectors[move]);

            return -similarity;
        }


        protected float statusWrap(GameState state, float s) {
            float healthScore  = System.Math.Abs(state.Health);

            var healthWeight = 0f;

            var score = s  + (healthWeight * healthScore);

            if (state.PlayStatus.isDied () ||
                state.Player(pId).BottomBoundary < state.Platforms.Min(x => x.TopBoundary)) {
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

        override public float EstimateScore(GameState state, Input input) {
            GameObject target = NextPlatform (state);
            return EstimateScore (state, input, target.Target);
        }


        override public float Score(GameState state) {
            Player player = state.Player (pId);

            GameObject next = NextPlatform (state);

            var dist =
                Vector2.Distance (player.SurfaceCenter, next.Target) +
                PlatformPathDistance (state, pId, next);
            
            //            Debug.WriteLine("{0} {1}",
            //                Vector2.Distance (player.SurfaceCenter, nextWaypoint),
            //                Vector2.Distance (nextWaypoint, state.Goal.SurfaceCenter));

            return statusWrap(state, dist);
//            return dist;
        }
    }

    public class LinearHeuristic : Heuristic {
        public LinearHeuristic(PlayerId pId) : base(pId)  {}

        override public float EstimateScore(GameState state, Input input) {
            return EstimateScore (state, input, state.Goal.Target);
        }

        override public float Score(GameState state) {
            float goalDistance = Vector2.Distance(state.Player(pId).Coords, state.Goal.Coords);
            float healthScore  = System.Math.Abs(state.Health);

            var healthWeight = 0f;

            return goalDistance + (healthWeight * healthScore);
        }
    }
}

