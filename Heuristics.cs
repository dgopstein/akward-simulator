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

        abstract public float Score (List<Platform> path, GameState state);
        abstract public float EstimateScore (List<Platform> path, GameState state, Input move);

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

//        Profiler prof = new Profiler ("EstimateScore", 30000, 3);
        private Vector2 gravity = new Vector2 (0, -PlatformAStar.MaxReachY);
        protected float EstimateScore(GameState state, Input move, Vector2 target) {
//            prof.Start ();

            float similarity;

            if (move == Input.Noop) {
                similarity = 0f;
            } else {
                var player = state.Player (pId);

                Vector2 gravOffset = state.IsGrounded (player) ? Vector2.Zero : gravity;

                Vector2 targetVector = Vector2.Subtract (target, player.Target + gravOffset);

                Vector2.Normalize (targetVector);

//                Debug.WriteLine("targetVector: {0}", gravOffset);

                similarity = (float)Util.CosineSimilarity(targetVector, moveVectors [move]);
            }

//            prof.End ();

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

        Platform currentNextPlatform;
        public Platform CurrentNextPlatform { get { return CurrentNextPlatform; } }

        public WaypointHeuristic(GameState initialState, PlayerId pId) : base(pId) {
//            this.initialState = initialState;
            pas = new PlatformAStar (initialState.Platforms);
        }

        public GameObject NextPlatform(List<Platform> path, GameState state) {
            return pas.NextPlatform (path, state.Player(pId), state.Goal);
        }

        private float PlatformPathDistance(GameState state, PlayerId pId, GameObject platform) {
            var path = pas.PlatformPath (state.Player (pId), state.Goal).SkipWhile(x => x != platform).ToArray();

            float dist = 0f;
            for (int i = 0; i < path.Count () - 1; i++) {
                dist += path [i].Distance (path [i + 1]);
            }

            return dist;
        }

        override public float EstimateScore(List<Platform> path, GameState state, Input input) {
            GameObject target = NextPlatform (state);
            return EstimateScore (state, input, target.Target);
        }


        override public float Score(List<Platform> path, GameState state) {
            Player player = state.Player (pId);

            GameObject next = NextPlatform (state);
            currentNextPlatform = next;

            var dist =
                Vector2.Distance (player.SurfaceCenter, next.Target) +
                PlatformPathDistance (state, pId, next);
            
//            return statusWrap(state, dist);
            return dist;
        }
    }

    public class LinearHeuristic : Heuristic {
        public LinearHeuristic(PlayerId pId) : base(pId)  {}

        override public float EstimateScore(List<Platform> path, GameState state, Input input) {
            return EstimateScore (state, input, state.Goal.Target);
        }

        override public float Score(List<Platform> path, GameState state) {
            float goalDistance = Vector2.Distance(state.Player(pId).Coords, state.Goal.Coords);
            float healthScore  = System.Math.Abs(state.Health);

            var healthWeight = 0f;

            return goalDistance + (healthWeight * healthScore);
        }
    }
}

