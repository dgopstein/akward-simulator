using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Microsoft.Xna.Framework;

using GobjPair = System.Tuple<awkwardsimulator.GameObject, awkwardsimulator.GameObject>;
using PlatPair = System.Tuple<awkwardsimulator.Platform, awkwardsimulator.Platform>;
using StateNode = awkwardsimulator.AStarNode<System.Tuple<awkwardsimulator.Platform, awkwardsimulator.Platform>, System.Tuple<awkwardsimulator.Platform, awkwardsimulator.Platform>>;
using CombinedPlatformGraph = System.Collections.Generic.Dictionary<System.Tuple<awkwardsimulator.Platform, awkwardsimulator.Platform>, System.Collections.Generic.HashSet<System.Tuple<awkwardsimulator.Platform, awkwardsimulator.Platform>>>;


namespace awkwardsimulator
{
    public class CombinedPlatformAStar
    {
        private CombinedPlatformGraph platformGraph;
        public CombinedPlatformGraph PlatformGraph { get { return platformGraph; } }

        private List<Platform> Platforms { get { return platformGraph.Keys.Select(x => x.Item1).Distinct().ToList(); } }

        private static CombinedPlatformGraph BuildCombinedPlatformGraph(List<Platform> platforms) {
            var platGraph = PlatformUtil.BuildPlatformGraph (platforms);

//            Debug.Print ("PlatGraph: {0}", PlatformUtil.PlatGraphStr (platGraph));

            var cross =
                from pair1 in platGraph
                            from pair2 in platGraph
                            from target1 in pair1.Value
                            from target2 in pair2.Value
                            select Tuple.Create (Tuple.Create (pair1.Key, pair2.Key), Tuple.Create (target1, target2));

            Dictionary<PlatPair, HashSet<PlatPair>> dict = cross.GroupBy (x => x.Item1)
                .ToDictionary(x => x.Key, x=> x.Select(y => y.Item2).ToHashSet());

            return dict;
        }

        public CombinedPlatformAStar(List<Platform> platforms) {
            this.platformGraph = BuildCombinedPlatformGraph (platforms);
        }

        private bool shouldGoNext(Player player, GameObject plat0, GameObject plat1) {
        
            var between0and1 = plat1.Distance(plat0) > plat1.Distance(player);
            var closeEnough = player.Distance(plat0) < (1 * Player.Size.X) && player.Y >= plat0.Y;
            var unreachable1 = PlatformUtil.unreachable (Platforms, player, plat1);
            var bothAbove = plat1.TopBoundary > player.BottomBoundary && plat0.TopBoundary > player.BottomBoundary;


            return !bothAbove && !unreachable1 && (between0and1 || closeEnough);
        }

        public Tuple<GameObject, GameObject> NextPlatform(Player player1, Player player2, GameObject end1, GameObject end2) {
            var path = CombinedPlatformPath (player1, player2, end1, end2);

            Tuple<GameObject, GameObject> next;

            if (path.Count == 1) {
                next = path.First ();
            } else {
                next = shouldGoNext(player1, path[0].Item1, path[1].Item1) &&
                       shouldGoNext(player2, path[0].Item2, path[1].Item2) ? path[0] : path[1];
            }

            return next;
        }

        PlatPair g2p(GobjPair gp) {
            return Tuple.Create ((Platform)gp.Item1, (Platform)gp.Item2);
        }

        GobjPair p2g(PlatPair gp) {
            return Tuple.Create ((GameObject)gp.Item1, (GameObject)gp.Item2);
        }

        Dictionary<Tuple<Player, Player, GameObject, GameObject>, List<Tuple<GameObject, GameObject>>> paths =
            new Dictionary<Tuple<Player, Player, GameObject, GameObject>, List<Tuple<GameObject, GameObject>>> ();

        public List<Tuple<GameObject, GameObject>> CombinedPlatformPath(
            Player start1, Player start2, GameObject end1, GameObject end2) {

            var cacheKey = Tuple.Create (start1, start2, end1, end2);

            List<Tuple<GameObject, GameObject>> path;

            if (paths.ContainsKey(cacheKey)) {
                path = paths[cacheKey];
            } else {
                var startPlat1 = PlatformUtil.nearestReachablePlatform (start1, Platforms);
                var startPlat2 = PlatformUtil.nearestReachablePlatform (start2, Platforms);

//                var endReachablePlatforms1 = Platforms.FindAll (p => PlatformUtil.adjacent (Platforms, p, end1));
//                var endReachablePlatforms2 = Platforms.FindAll (p => PlatformUtil.adjacent (Platforms, p, end2));
//
//                Debug.WriteLineIf (endReachablePlatforms1.Count == 0, "No platforms within reach of the 1st goal!");
//                Debug.WriteLineIf (endReachablePlatforms2.Count == 0, "No platforms within reach of the 2nd goal!");
//
//                var end1Plat = PlatformUtil.nearestPlatform (end1.Center, endReachablePlatforms1);
//                var end2Plat = PlatformUtil.nearestPlatform (end2.Center, endReachablePlatforms2);

                path = runAStar (startPlat1, startPlat2, end1, end2).Select(p2g).ToList();

                path.Add(Tuple.Create(end1, end2));

                paths[cacheKey] = path;
            }

            return path;
        }

        double combinedPlatformHeuristic(StateNode node, GameObject end1, GameObject end2) {
            var endDist1 = Vector2.Distance (node.Value.Item1.SurfaceCenter, end1.SurfaceCenter);
            var endDist2 = Vector2.Distance (node.Value.Item2.SurfaceCenter, end2.SurfaceCenter);

            var platDist = Vector2.Distance (node.Value.Item1.Center, node.Value.Item2.Center);

            return endDist1 + endDist2 + 5 * Math.Abs (HealthControl.IdealDistance - platDist);
        }

        private List<Tuple<Platform, Platform>> runAStar(
            Platform start1, Platform start2, GameObject end1, GameObject end2) {

            int maxIters = 20;

            var paths = new SortedDictionary<double, StateNode>();

            var startPair = Tuple.Create (start1, start2);
//            var endPair = Tuple.Create (end1, end2);

            var root = new StateNode (null, startPair, startPair);
            paths.Add(combinedPlatformHeuristic(root, end1, end2), root);
            var best = root;

            for (int i = 0; i < maxIters &&
                !(PlatformUtil.adjacent(Platforms, best.Value.Item1, end1) &&
                  PlatformUtil.adjacent(Platforms, best.Value.Item2, end2)) &&
                paths.Count > 0; i++) {
                best.Children = PlatformGraph [best.Value].ToDictionary (x => x, x => new StateNode(best, x, x));

                foreach (var c in best.Children) {
                    var h = combinedPlatformHeuristic (c.Value, end1, end2);
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
