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
        public static readonly Dictionary<Input, Vector2> moveVectors =
            new Dictionary<Input, Vector2>(){
            { Input.Noop, new Vector2(0, 0) },
            { Input.Up, new Vector2(0, 1) },
            { Input.Right, new Vector2(1, 0) },
            { Input.UpRight, new Vector2(diagonangle, diagonangle) },
            { Input.Left, new Vector2(-1, 0) },
            { Input.UpLeft, new Vector2(-diagonangle, diagonangle) },
        };

//        Profiler prof = new Profiler ("EstimateScore", 30000, 3);
        public static readonly Vector2 gravVector = new Vector2 (0, -PlatformUtil.MaxReachY);
        protected float EstimateScore(GameState state, Input move, Vector2 target) {
            return EstimateScore (this.pId, state, move, target);
        }

        public static float EstimateScore(PlayerId pId, GameState state, Input move, Vector2 target) {
//            prof.Start ();

            float similarity;

            if (move == Input.Noop) {
                similarity = 0f;
            } else {
                var player = state.Player (pId);

                Vector2 gravOffset = state.IsGrounded (player) ? Vector2.Zero : gravVector;

                Vector2 targetVector = Vector2.Subtract (target, player.Target + gravOffset);

                Vector2.Normalize (targetVector);

//                Debug.WriteLine("targetVector: {0}", gravOffset);

                similarity = (float)Util.CosineSimilarity(targetVector, moveVectors [move]);
            }

//            prof.End ();

            return -similarity;
        }

        public float statusWrap(GameState state, float s) {
            float healthScore  = System.Math.Abs(state.Health);

            var healthWeight = 150f;

            var score = s  + (healthWeight * healthScore);

            if (state.PlayStatus.isDied () ||
                state.Player(pId).BottomBoundary < state.Platforms.Min(x => x.TopBoundary)) {
                score = 5*GameState.Width;
            } else if (state.PlayStatus.isWon ()) {
                score = 0;
            }

            return score;
        }
    }

    public class WaypointHeuristic : Heuristic {
//        readonly GameState initialState;
        public readonly PlatformAStar pas;

        public WaypointHeuristic(GameState initialState, PlayerId pId) : base(pId) {
//            this.initialState = initialState;
            pas = new PlatformAStar (initialState.Platforms);
        }

        public GameObject NextPlatform(GameState state) {
            return pas.NextPlatform (state.Player(pId), state.Goal);
        }

        private float PlatformPathDistance(GameState state, PlayerId pId, GameObject platform) {
            return PlatformPathDistance (pas.PlatformPath (state.Player (pId), state.Goal), platform);
        }

        public static float PlatformPathDistance(IEnumerable<GameObject> path, GameObject platform) {
            var remainingPath = path.SkipWhile(x => x != platform).ToArray();

            float dist = 0f;
            for (int i = 0; i < remainingPath.Count () - 1; i++) {
                dist += remainingPath [i].Distance (remainingPath [i + 1]);
            }

            return dist;
        }

        override public float EstimateScore(GameState state, Input input) {
            GameObject target = NextPlatform (state);
            return EstimateScore (state, input, target.Target);
        }


        override public float Score(GameState state) {
            GameObject next = NextPlatform (state);

            var dist = PlayerDistance(pas, state, pId, next);
            
            return statusWrap(state, dist);
//            return dist;
        }

        public static float PlayerDistance(PlatformAStar pas, GameState state, PlayerId pId, GameObject target) {
            return Vector2.Distance (state.Player(pId).SurfaceCenter, target.Target) +
                PlatformPathDistance (pas.PlatformPath (state.Player (pId), state.Goal), target);
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

