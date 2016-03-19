using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using FarseerPhysics.Common;

using PlatformGraph = System.Collections.Generic.Dictionary<awkwardsimulator.Platform, System.Collections.Generic.HashSet<awkwardsimulator.Platform>>;
using MoreLinq;

namespace awkwardsimulator
{
    public static class PlatformUtil
    {
        public static float distanceScore(Vector2 point, Platform plat) {
            var delta = Vector2.Subtract (plat.Center, point);

            delta *= new Vector2 (1.5f, 1); // weight X distance more than Y distance

            return delta.Length ();
        }

        public  static Platform nearestPlatform(Vector2 point, List<Platform> platforms) {
            return platforms.MinBy (plat => distanceScore(point, plat));
        }

        private static Vector2 nearestPoint(Vector2 point, List<Vector2> list) {
            return list.MinBy (p => Vector2.Distance (point, p));
        }

        public static PlatformGraph BuildPlatformGraph(List<Platform> platforms) {
            PlatformGraph platGraph = new PlatformGraph ();

            foreach (var plat1 in platforms) {
                HashSet<Platform> hs = new HashSet<Platform> ();
                foreach (var plat2 in platforms) {
                    if (plat1 != plat2 && adjacent (platforms, plat1, plat2)) {
                        hs.Add (plat2);
                    }
                }
                platGraph.Add(plat1, hs);
            }

            return platGraph;                
        }

        public static string PlatListStr<T>(List<T> platforms) where T : GameObject {
            return string.Join (", ", platforms.Select (x => x.Coords));
        }

        public static string PlatGraphStr(PlatformGraph platGraph) {
            string s = "";
            foreach (var entry in platGraph) {
                s += String.Format ("{0}[{1}]    ", entry.Key.Coords, PlatListStr(entry.Value.ToList()));
            }
            return s;
        }

        public static bool isLineOfSight(List<Platform> plats, GameObject go1, GameObject go2) {
            // We expect to intersect the start/end platforms, but no others

            var otherPlats = plats.FindAll (p => p != go1 && p != go2);

            var nIntersections = otherPlats.Count(plat => {
                Vector2 pt;
                var bl = plat.BottomLeft;
                var br = plat.BottomRight;
                var tr = plat.TopRight;
                var c1 = go1.Center;
                var c2 = go2.Center;

                var intersectBottom = LineTools.LineIntersect2(
                    ref bl, ref br, ref c1, ref c2, out pt);
                var intersectRight = LineTools.LineIntersect2(
                    ref br, ref tr, ref c1, ref c2, out pt);

                return intersectBottom || intersectRight;
            });

            return 0 == nIntersections;
        }

        const int MaxReachX = 20;
        public static readonly int MaxReachY = 15;
        public static bool adjacent(List<Platform> plats, GameObject go1, GameObject go2) {

            var dists =
                go1.Surface.SelectMany (a =>
                    go2.Surface.Select (b => Vector2.Subtract (a, b)));

            var closeEnough = dists.Any (d => Math.Abs (d.X) <= MaxReachX && Math.Abs (d.Y) <= MaxReachY);

            return closeEnough && isLineOfSight (plats, go1, go2); //TODO add element
        }

        public static float remainingJumpDist(Player player) {
            return (float)Math.Pow (player.Velocity.Y, 2) / (2f * 50f); //TODO 50 is a guess...
        }

        public static bool unreachable(List<Platform> platforms, Player player, GameObject plat) {
            var ret = false;

            var vert = plat.SurfaceCenter.Y - player.BottomBoundary;

            if (vert > 0) { // Platform is above you
                if (GameState.IsGrounded (platforms, player)) {
                    if (vert > MaxReachY) {
                        ret = true;
                    }
                } else {
                    if (vert > remainingJumpDist(player)) { // You won't jump high enough
                        ret = true;
                    }
                }
            }

            return ret;
        }

        public static Platform nearestReachablePlatform(Player player, List<Platform> platforms) {
            // Eliminate platforms we've fallen below
            var lowerPlats = platforms.FindAll (plat => !PlatformUtil.unreachable(platforms, player, plat));

            Platform nearest;
            if (lowerPlats.Count > 0) {
                nearest = nearestPlatform (player.SurfaceCenter, lowerPlats);
            } else {
                nearest = platforms.First ();
            }

            return nearest;
        }


    }
}

