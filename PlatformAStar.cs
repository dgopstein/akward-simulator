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
            this.platformGraph = PlatformUtil.BuildPlatformGraph (platforms);
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
                var unreachable1 = PlatformUtil.unreachable (Platforms, player, plat1);
                var bothAbove = plat1.TopBoundary > player.BottomBoundary && plat0.TopBoundary > player.BottomBoundary;

                next = (!bothAbove && !unreachable1 && (between0and1 || closeEnough)) ? plat1 : plat0;
            }

            return next;
        }


        Dictionary<Tuple<Player, GameObject>, List<GameObject>> paths =
            new Dictionary<Tuple<Player, GameObject>, List<GameObject>> ();

        public List<GameObject> PlatformPath(Player start, GameObject end) {
            var tup = Tuple.Create (start, end);

            List<GameObject> path;

            if (paths.ContainsKey(tup)) {
                path = paths[tup];
            } else {
                var startPlat = PlatformUtil.nearestReachablePlatform (start, Platforms);

                var endReachablePlatforms = Platforms.FindAll (p => PlatformUtil.adjacent (Platforms, p, end));

                Debug.WriteLineIf (endReachablePlatforms.Count == 0, "No platforms within reach of the goal!");

                var endPlat = PlatformUtil.nearestPlatform (end.Center, endReachablePlatforms);

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
    }
}
