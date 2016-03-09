using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using MoreLinq;

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
            var nearest = nearestPlatform (player, Platforms);

//            Debug.WriteLine ("{0}", nearest);


            var path = PlatformPath (nearest, end);

            GameObject next;

            if (path.Count == 1) {
                next = path.First ();
            } else {
                var plat0 = path [0];
                var plat1 = path [1];

                var between0and1 = plat1.Distance(plat0) > plat1.Distance(player);
                var closeEnough = player.Distance(plat0) < (1 * Player.Size.X) && player.Y >= plat0.Y;
                var unreachable1 = unreachable (player, plat1);
                var bothAbove = plat1.Top > player.Bottom && plat0.Top > player.Bottom;

//                Debug.WriteLine ("");
//                Debug.WriteLine ("d1:{0}, d2:{1}", plat1.Distance(plat0), plat1.Distance(player));
//                Debug.WriteLine ("btwn:{0}, close:{1}", between0and1?"T":"F", closeEnough?"T":"F");

                next = ((!bothAbove || !unreachable1) && (between0and1 || closeEnough)) ? plat1 : plat0;
            }

            return next;
        }

        private bool grounded(Player player) {
            return Platforms.Any (plat => {

                var horizontal =
                    plat.Right >= player.Left &&
                    plat.Left <= player.Right;

                var vertical = Math.Abs (player.Bottom - plat.Top) < 0.1;

                return horizontal && vertical;
            });
        }

        public static float remainingJumpDist(Player player) {
            return (float)Math.Pow (player.Velocity.Y, 2) / (2f * 50f); //TODO 50 is a guess...
        }


//        //todo: return all platforms and an index because we need to be able to see where we came from
        public List<GameObject> PlatformPath(GameObject start, GameObject end) {
            var startPlat = nearestPlatform (start.SurfaceCenter, Platforms);

            var endReachablePlatforms = Platforms.FindAll (p => adjacent (p, end));

//            Debug.WriteLine ("all platforms: "+ PlatListStr (Platforms));
//            Debug.WriteLine ("end reachable: "+ PlatListStr (endReachablePlatforms));

            var endPlat = nearestPlatform (end.Center, endReachablePlatforms);

            return PlatformPath (startPlat, endPlat).Concat(end).ToList<GameObject>();
        }

        private List<Platform> PlatformPath(Platform start, Platform end) {
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

        private Platform nearestPlatform(Player player, List<Platform> platforms) {
            // Eliminate platforms we've fallen below
//            var lowerPlats = platforms.FindAll (plat => !(player.Velocity.Y <= 0 && plat.Y > player.SurfaceCenter.Y));

            var lowerPlats = platforms.FindAll (plat => plat.Y <= player.SurfaceCenter.Y);
//            Debug.WriteLine(string.Join(", ", lowerPlats.Select(x => x.ToString())));
//            var lowerPlats = platforms;

            Platform nearest;

            if (lowerPlats.Count > 0) {
                nearest = lowerPlats.MinBy (plat => distanceScore(player.SurfaceCenter, plat));
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
                    if (plat1 != plat2 && adjacent (plat1, plat2)) {
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

        const int MaxReachX = 20;
        const int MaxReachY = 15;
        private static bool adjacent(GameObject go1, GameObject go2) {
            return go1.Corners.SelectMany ( a =>
                go2.Corners.Select( b => Vector2.Subtract (a, b)) )
                .Any (d => Math.Abs (d.X) <= MaxReachX && Math.Abs (d.Y) <= MaxReachY);
        }

        private bool unreachable(Player player, GameObject plat) {
            var ret = false;

            var vert = plat.Top - player.Bottom;

            if (vert > 0) { // Platform is above you
                if (grounded (player)) {
                    if (vert > MaxReachY) {
                        ret = true;
                    }
                } else {
                    if ((vert/2f) > (player.Velocity.Y * .4)) { // You won't jump high enough
                        ret = true;
                    }
                }
            }

            if (plat.Bottom == 40) {
                Debug.WriteLine ("unreachable: {0} {1} {2}", ret, vert, player.Velocity.Y);
            }

            return ret;
        }
    }
}

