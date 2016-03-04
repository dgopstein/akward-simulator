﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;


using StateNode = awkwardsimulator.AStarNode<awkwardsimulator.GameState, awkwardsimulator.Input>;
using Microsoft.Xna.Framework;

namespace awkwardsimulator
{
    public class WaypointAStar : AStar
    {
        override protected int maxIters() {return 50;}

        PlatformAStar pas;

        public WaypointAStar (GameState state, PlayerId pId) : base(state, pId) {
            pas = new PlatformAStar (state.Platforms);
        }

        public Vector2 NextWaypoint(GameState state) {
            var pc = this.thisPlayer (state).SurfaceCenter;
            return pas.NextPlatform (pc, state.Goal.Center).SurfaceCenter;
        }

        override protected Func<StateNode, double>  heuristicGenerator() {
            int uniqueId = 1;

            Func<StateNode, double> heuristic = (s) => {
                var pc = this.thisPlayer (s.Value).SurfaceCenter;

                var nextWaypoint = NextWaypoint(s.Value);

//                Debug.WriteLine("{0} {1}", pc, nextWaypoint);

                double h = Heuristics.SqrtDistance (pc, nextWaypoint) + Vector2.Distance(nextWaypoint, s.Value.Goal.Center);


                h =  Math.Truncate(h * 1000d) / 1000d; // remove fractional noise
                h += (0.0000001 * uniqueId++); // add marginal unique id to avoid collisions

                h += 0.1 * s.Depth(); // discourage long paths

                return h;
            };

            return heuristic;
        }
    }
}