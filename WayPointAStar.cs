using System;
using System.Collections.Generic;
using System.Linq;
//using System.Diagnostics;
//using Microsoft.Xna.Framework;
//
//
//using StateNode = awkwardsimulator.AStarNode<awkwardsimulator.GameState, awkwardsimulator.Input>;
//using Heuristic = System.Func<awkwardsimulator.GameState, awkwardsimulator.Player, float>;
//
//
//namespace awkwardsimulator
//{
//    public class WaypointAStar : AStar
//    {
//        override protected int maxIters() {return 50;}
//
//        PlatformAStar pas;
//
//        public WaypointAStar (GameState state, PlayerId pId, Heuristic heuristic) :
//                base(state, pId, heuristic) {
//            pas = new PlatformAStar (state.Platforms);
//        }
//
//        public Vector2 NextWaypoint(GameState state) {
//            return pas.NextPlatform (this.thisPlayer (state), state.Goal).SurfaceCenter;
//        }
//
//        override protected Func<StateNode, Player, double>  scorerGenerator(Heuristic heuristic) {
//                
//            int uniqueId = 1;
//
//            Func<StateNode, Player, double> scorer = (s, p) => {
//                    
//                var pc = p.SurfaceCenter;
//
//                var nextWaypoint = NextWaypoint(s.Value);
//
//                double h = Heuristics.SqrtDistance(s.Value, pc, nextWaypoint) +
//                           Vector2.Distance(nextWaypoint, s.Value.Goal.Center);
//
//                h =  Math.Truncate(h * 1000d) / 1000d; // remove fractional noise
//                h += (0.0000001 * uniqueId++); // add marginal unique id to avoid collisions
//
//                h += 0.1 * s.Depth(); // discourage long paths
//
//                return h;
//            };
//
//            return scorer;
//        }
//    }
//}