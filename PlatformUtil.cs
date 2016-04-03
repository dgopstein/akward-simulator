using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using FarseerPhysics.Common;

using PlatformGraph = System.Collections.Generic.Dictionary<awkwardsimulator.Platform, System.Collections.Generic.HashSet<awkwardsimulator.Platform>>;
using MoreLinq;
using System.Diagnostics;

namespace awkwardsimulator
{
    public static class PlatformUtil
    {
        public static float distanceScore(Vector2 point, Platform plat) {
            var delta = Vector2.Subtract (plat.Center, point);

            delta *= new Vector2 (1.5f, 1); // weight X distance more than Y distance

            return delta.Length ();
        }

        public static IEnumerable<Platform> platsBelow(IEnumerable<Platform> plats, GameObject obj) {
            return plats.Where (x => x.SurfaceCenter.Y <= obj.SurfaceCenter.Y);
        }

        public  static Platform nearestPlatform(Vector2 point, IEnumerable<Platform> platforms) {
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
                    if (/*plat1 != plat2 &&*/ adjacent (platforms, plat1, plat2)) {
                        hs.Add (plat2);
                    }
                }
                platGraph.Add(plat1, hs);
            }

            return platGraph;                
        }

        public static IEnumerable<Platform> Subdivide(float largest, Platform plat) {
            var nDivs = Math.Ceiling (plat.W / largest);
            var newWidth = plat.W / nDivs;

            var subDivPlats = new List<Platform> ();
            float offset = 0;
            for (int i = 0; i < nDivs; i++) {
                var name = plat.Name + "" + i;
                var coords = new Vector2(plat.X + offset, plat.Y);
                var size = new Vector2((float)newWidth, (float)plat.H);

                offset += (float)newWidth;

                subDivPlats.Add (new Platform(name, coords, size));
            }

            return subDivPlats;
        }

        public static IEnumerable<Platform> Subdivide(IEnumerable<Platform> plats, float largest = 10f) {
            return plats.SelectMany(p => Subdivide(largest, p));
        }

        public static string PlatListStr<T>(IEnumerable<T> platforms) where T : GameObject {
            return string.Join (", ", platforms.Select (x => x.ToString()));
        }

        public static string PlatPairListStr<T>(IEnumerable<Tuple<T, T>> platforms) where T : GameObject {
            return string.Join(", ", platforms.Select(tup1 => tup1.Item1 + "|" + tup1.Item2));
        }

        public static string PlatGraphStr(PlatformGraph platGraph) {
            return string.Join(", ", 
                platGraph.Select(kv => 
                    String.Format ("{0}[{1}]", kv.Key.ToString(), PlatListStr(kv.Value.ToList()))));
        }

        public static bool isLineOfSight(IEnumerable<Platform> plats, GameObject go1, GameObject go2) {
            // We expect to intersect the start/end platforms, but no others
            var otherPlats = plats.Where (p => p != go1 && p != go2);
        
            return isLineOfSight (otherPlats, go1.Target, go2.Target);
        }


        public static bool isLineOfSight(IEnumerable<Platform> plats, Vector2 target1, Vector2 target2) {
            var hasIntersections = plats.Any(plat => {
                var bl = plat.BottomLeft;
                var br = plat.BottomRight;
                var tl = plat.TopLeft;
                var tr = plat.TopRight;

                var sides = new List<Tuple<Vector2, Vector2>>() {
                    Tuple.Create(bl, br),
                    Tuple.Create(br, tr),
                    Tuple.Create(tr, tl),
                    Tuple.Create(tl, bl)
                };

                Vector2 pt;
                var anyIntersections = sides.Any(tup => {
                    var c1 = tup.Item1;
                    var c2 = tup.Item2;

//                    var isOnLine = 
//                        LineTools.DistanceBetweenPointAndLineSegment(ref target1, ref c1, ref c2) < 0.01 ||
//                        LineTools.DistanceBetweenPointAndLineSegment(ref target2, ref c1, ref c2) < 0.01 ;
      
                    var intersectsLine = LineTools.LineIntersect2(
                        ref c1, ref c2, ref target1, ref target2, out pt);

                    return intersectsLine; // || isOnLine;
                });

                return anyIntersections;
            });

            return !hasIntersections;
        }

//        private static bool isLineOfSightAccessible(List<Platform> plats, GameObject start, GameObject end) {
//            // We expect to intersect the start/end platforms, but no others
//
//            var otherPlats = plats.FindAll (p => p != start && p != end);
//
//            var areIntersections = otherPlats.Any(plat => {
//                Vector2 pt;
//                var bl = plat.BottomLeft;
//                var br = plat.BottomRight;
//                var tl = plat.TopLeft;
//                var tr = plat.TopRight;
//                var c1 = start.Target;
////                var c2 = end.Target;
//
//                var targets = new List<Vector2>() {
//                    end.TopLeft,// + Player.Size,
//                    end.TopRight,// + Player.Size*new Vector2(-1, 1)
//                };
//
//                var intersectsAllEntries = targets.All(c2 => {
//                    var intersectBottom = LineTools.LineIntersect2(
//                        ref bl, ref br, ref c1, ref c2, out pt);
//                    var intersectRight = LineTools.LineIntersect2(
//                        ref br, ref tr, ref c1, ref c2, out pt);
//                    var intersectTop = LineTools.LineIntersect2(
//                        ref tr, ref tl, ref c1, ref c2, out pt);
//                    var intersectLeft = LineTools.LineIntersect2(
//                        ref tl, ref bl, ref c1, ref c2, out pt);
//
//                    return intersectBottom || intersectRight || intersectTop || intersectLeft;
//                });
//
//                return intersectsAllEntries;
//            });
//
//            return !areIntersections;
//        }

        const int MaxReachX = 20;
        public static readonly int MaxReachY = 15;
        public static bool adjacent(IEnumerable<Platform> plats, GameObject start, GameObject end) {

            var surfaceCross =
                start.ExitSurface.SelectMany (a =>
                    end.AccessSurface.Select (b => Tuple.Create(a + Player.Size * new Vector2(0, 1), b)));

            var closeAndVisibleEnough = surfaceCross.Any (tup => {
                var a = tup.Item1;
                var b = tup.Item2;
                var dist = Vector2.Subtract (a, b);
                var closeEnough = Math.Abs (dist.X) <= MaxReachX && Math.Abs (dist.Y) <= MaxReachY;
                var topLeftVisible =  isLineOfSight(plats, a, b);
                var topRightVisible = isLineOfSight(plats, a, b);

                if (start.ToString() + " " + end.ToString() == "c_0 d_1") {
                    Console.WriteLine("v1sab1l1ty: {0}->{1} - {2}, {3}, {4}", start, end, closeEnough, topLeftVisible, topRightVisible);
                }

                return closeEnough && (topLeftVisible || topRightVisible);
            });

            return closeAndVisibleEnough; //TODO add element
        }

        public static float remainingJumpDist(Player player) {
            return (float)Math.Pow (player.Velocity.Y, 2) / (2f * 50f); //TODO 50 is a guess...
        }

        public static bool unreachable(IEnumerable<Platform> platforms, Player player, GameObject plat) {
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

        public static Platform nearestReachablePlatform(Player player, IEnumerable<Platform> platforms) {
            // Eliminate platforms we've fallen below
            var lowerPlats = platforms.Where (plat => !PlatformUtil.unreachable(platforms, player, plat));

            Platform nearest;
            if (lowerPlats.Count() > 0) {
                nearest = nearestPlatform (player.SurfaceCenter, lowerPlats);
            } else {
                nearest = platforms.First ();
            }

            return nearest;
        }


    }
}

