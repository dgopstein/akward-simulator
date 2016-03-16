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
    public class SubPlatformPathFinder
    {
        private PlatformGraph platformGraph;
        public PlatformGraph PlatformGraph { get { return platformGraph; } }

        private List<Platform> Platforms { get { return platformGraph.Keys.ToList(); } }

        private PlatformAStar pas;

        public SubPlatformPathFinder(List<Platform> platforms) {
            pas = new PlatformAStar (platforms);

        }
    }
}
