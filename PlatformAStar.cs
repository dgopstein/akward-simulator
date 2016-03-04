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

        public PlatformAStar(List<Platform> platforms) {
            this.platformGraph = BuildPlatformGraph (platforms);
        }

        public List<Platform> PlatformPath(Vector2 start, Vector2 end) {
            var startPlat = nearest (start);
            var endPlat = nearest (end);

            return PlatformPath (startPlat, endPlat);
        }

        private List<Platform> PlatformPath(Platform start, Platform end) {
            int maxIters = 20;

            var paths = new SortedDictionary<double, StateNode>();

            Func<StateNode, double> heuristic = p => Vector2.Distance (p.Value.Center, end.Center);

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

//            var res = best.FirstAncestor ().Input;
//            return res;
            return best.ToPath().Select(tup => tup.Item2).ToList();
        }

        private Platform nearest(Vector2 point) {
            return PlatformGraph.Keys.ToList()
                .FindAll(plat => plat.Y <= point.Y) // Don't return platforms above the given point
                .MinBy(plat => Vector2.Distance(plat.Center, point));
        }

        private static PlatformGraph BuildPlatformGraph(List<Platform> platforms) {
            PlatformGraph platGraph = new PlatformGraph ();

            foreach (var plat1 in platforms) {
                HashSet<Platform> hs = new HashSet<Platform> ();
                foreach (var plat2 in platforms) {
                    if (plat1 != plat2 && reachable (plat1, plat2)) {
                        hs.Add (plat2);
                    }
                }
                platGraph.Add(plat1, hs);
            }

            return platGraph;                
        }

        public static string PlatListStr(List<Platform> platforms) {
            return string.Join (", ", platforms.Select (x => x.Coords));
        }

        public static string PlatGraphStr(PlatformGraph platGraph) {
            string s = "";
            foreach (var entry in platGraph) {
                s += String.Format ("{0}[{1}]    ", entry.Key.Coords, PlatListStr(entry.Value.ToList()));
            }
            return s;
        }

        private static bool reachable(GameObject go1, GameObject go2) {
            int maxX = 20, maxY = 15;

            return go1.Corners ().SelectMany ( a =>
                go2.Corners().Select( b => Vector2.Subtract (a, b)))
                .Any (d => Math.Abs (d.X) <= maxX && Math.Abs (d.Y) <= maxY);
        }
    }
}

