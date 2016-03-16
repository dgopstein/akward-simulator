using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using MoreLinq;
using FarseerPhysics.Common;

using PlatformGraph = System.Collections.Generic.Dictionary<awkwardsimulator.Platform, System.Collections.Generic.HashSet<awkwardsimulator.Platform>>;
using StateNode = awkwardsimulator.AStarNode<awkwardsimulator.Platform, awkwardsimulator.Platform>;

namespace awkwardsimulator
{
    public class PlatformAStar
    {
        private PlatformGraph platformGraph;
        public PlatformGraph PlatformGraph { get { return platformGraph; } }

        private List<Platform> Platforms { get { return platformGraph.Keys.ToList(); } }

        public PlatformAStar(List<Platform> platforms) {
            this.platformGraph = BuildPlatformGraph (platforms);
        }

        public GameObject NextPlatform(Player player, GameObject end) {
            var path = PlatformPath (player, end);

            GameObject next;

            if (path.Count == 1) {
                next = path.First ();
            } else {
                var plat0 = path [0];
                var plat1 = path [1];

                var between0and1 = plat1.Distance(plat0) > plat1.Distance(player);
                var closeEnough = player.Distance(plat0) < (1 * Player.Size.X) && player.Y >= plat0.Y;
                var unreachable1 = unreachable (Platforms, player, plat1);
                var bothAbove = plat1.TopBoundary > player.BottomBoundary && plat0.TopBoundary > player.BottomBoundary;

//                Debug.WriteLine ("");
//                Debug.WriteLine ("d1:{0}, d2:{1}", plat1.Distance(plat0), plat1.Distance(player));
//                Debug.WriteLine ("btwn:{0}, close:{1}", between0and1?"T":"F", closeEnough?"T":"F");

                next = (!bothAbove && !unreachable1 && (between0and1 || closeEnough)) ? plat1 : plat0;
            }

            return next;
        }

        public static float remainingJumpDist(Player player) {
            return (float)Math.Pow (player.Velocity.Y, 2) / (2f * 50f); //TODO 50 is a guess...
        }


        Dictionary<Tuple<Player, GameObject>, List<GameObject>> paths =
            new Dictionary<Tuple<Player, GameObject>, List<GameObject>> ();

        public List<GameObject> PlatformPath(Player start, GameObject end) {
            var tup = Tuple.Create (start, end);

            List<GameObject> path;

            if (paths.ContainsKey(tup)) {
                path = paths[tup];
            } else {
                var startPlat = nearestReachablePlatform (start, Platforms);

                var endReachablePlatforms = Platforms.FindAll (p => adjacent (Platforms, p, end));

                Debug.WriteLineIf (endReachablePlatforms.Count == 0, "No platforms within reach of the goal!");

    //            Debug.WriteLine ("all platforms: "+ PlatListStr (Platforms));
    //            Debug.WriteLine ("end reachable: "+ PlatListStr (endReachablePlatforms));

                var endPlat = nearestPlatform (end.Center, endReachablePlatforms);

                path = runAStar (startPlat, endPlat).Concat(end).ToList<GameObject>();

                paths[tup] = path;
            }

            return path;
        }

        private List<Platform> runAStar(Platform start, Platform end) {
            int maxIters = 20;

            var paths = new SortedDictionary<double, StateNode>();

            Func<StateNode, double> heuristic = p => Vector2.Distance (p.Value.SurfaceCenter, end.SurfaceCenter);

            var root = new StateNode (null, start, start);
            paths.Add(heuristic(root), root);
            var best = root;

            for (int i = 0; i < maxIters && best.Value != end && paths.Count > 0; i++) {
                best.Children = PlatformGraph [best.Value].ToDictionary (x => x, x => new StateNode(best, x, x));

                foreach (var c in best.Children) {
                    var h = heuristic (c.Value);
                    if (!paths.ContainsKey(h)) {
                        paths.Add (h, c.Value);
                    }
                }

                best = paths.First().Value;
                paths.Remove (paths.First().Key);
            }

            return best.ToPath().Select(tup => tup.Item2).ToList();
        }

        private float distanceScore(Vector2 point, Platform plat) {
            var delta = Vector2.Subtract (plat.Center, point);

            delta *= new Vector2 (1.5f, 1); // weight X distance more than Y distance

            return delta.Length ();
        }

        private Platform nearestReachablePlatform(Player player, List<Platform> platforms) {
            // Eliminate platforms we've fallen below
            var lowerPlats = platforms.FindAll (plat => !unreachable(platforms, player, plat));

            Platform nearest;
            if (lowerPlats.Count > 0) {
                nearest = nearestPlatform (player.SurfaceCenter, lowerPlats);
            } else {
                nearest = platforms.First ();
            }

            return nearest;
        }

        private Platform nearestPlatform(Vector2 point, List<Platform> platforms) {
            return platforms.MinBy (plat => distanceScore(point, plat));
        }

        private static Vector2 nearestPoint(Vector2 point, List<Vector2> list) {
            return list.MinBy (p => Vector2.Distance (point, p));
        }

        private static PlatformGraph BuildPlatformGraph(List<Platform> platforms) {
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


        protected static bool isLineOfSight(List<Platform> plats, GameObject go1, GameObject go2) {
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

//                Debug.WriteLine ("{0} - {1} nnIntersections: {2}", go1.Center, go2.Center, nIntersections);

            return 0 == nIntersections;
//            return true;
        }

        const int MaxReachX = 20;
        public static readonly int MaxReachY = 15;
        protected static bool adjacent(List<Platform> plats, GameObject go1, GameObject go2) {
//            var dist = Vector2.Subtract (go1.SurfaceCenter, go2.SurfaceCenter);
//            var closeEnough = Math.Abs (dist.X) <= MaxReachX && Math.Abs (dist.Y) <= MaxReachY;

            var dists =
                go1.Surface.SelectMany (a =>
                    go2.Surface.Select (b => Vector2.Subtract (a, b)));

            var closeEnough = dists.Any (d => Math.Abs (d.X) <= MaxReachX && Math.Abs (d.Y) <= MaxReachY);

            return closeEnough && isLineOfSight (plats, go1, go2); //TODO add element
        }

        protected bool unreachable(List<Platform> platforms, Player player, GameObject plat) {
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
    }
}

