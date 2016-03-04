using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using PlatformGraph = System.Collections.Generic.Dictionary<awkwardsimulator.Platform, System.Collections.Generic.HashSet<awkwardsimulator.Platform>>;
using System.Linq;
using System.Diagnostics;
using MoreLinq;

namespace awkwardsimulator
{
//    class PlatformNode {
//        private Platform platform;
//        public Platform Platform { get { return platform; } }
//
//        private List<Platform> neighbors;
//        public List<Platform> Neighbors { get { return neighbors; } }
//
//        public PlatformNode(Platform plaform, List<Platform> neighbors) {
//            this.platform = platform;
//            this.neighbors = neighbors;
//        }
//    }

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
            //TODO A* this
            return new List<Platform>();
        }

        private Platform nearest(Vector2 point) {
            return PlatformGraph.Keys.ToList()
                .FindAll(plat => plat.Y <= point.Y) // Don't return platforms above the given point
                .MinBy(plat => Vector2.Distance(plat.Center(), point));
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

        private static string PlatGraphStr(PlatformGraph platGraph) {
            string s = "";
            foreach (var entry in platGraph) {
                s += String.Format ("{0}[{1}]    ", entry.Key.Coords, string.Join (", ", entry.Value.Select (x => x.Coords)));
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

