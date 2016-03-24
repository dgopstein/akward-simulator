using System;
using NUnit.Framework;

using PlatGraph = System.Collections.Generic.Dictionary<awkwardsimulator.Platform, System.Collections.Generic.HashSet<awkwardsimulator.Platform>>;
using System.Collections.Generic;
using System.Linq;

namespace awkwardsimulator
{
    [TestFixture]
    public class UnitTests
    {
        [Test()]
        public void PlatformGraphLevel1() {
            PlatGraph generated = PlatformUtil.BuildPlatformGraph (Level.Level1.Platforms);

            string expected = "a[b], b[a, d], c[d], d[b, c]";

            Assert.AreEqual (expected, PlatformUtil.PlatGraphStr(generated));
        }

        [Test()]
        public void PlatformAdjacency() {
            var level = Level.Level1;

//            var a = level.Platform ("a");
            GameObject b = level.Platform ("b");
            GameObject c = level.Platform ("c");
            GameObject d = level.Platform ("d");
            GameObject goal = level.Goal;

            var list = new List<Tuple<bool, GameObject, GameObject>> () {
                Tuple.Create (false, b, c),
                Tuple.Create (true, b, d),
                Tuple.Create (true, c, d),
                Tuple.Create (true, c, goal),
                Tuple.Create (false, d, goal),
            };

            foreach (var tup in list) {
                Assert.AreEqual (tup.Item1,  PlatformUtil.adjacent(level.Platforms, tup.Item2, tup.Item3));
            }
        }

        [Test()]
        public void CombinedPlatformPath() {
            var level = Level.Level1;

            var cpas = new CombinedPlatformAStar (level.Platforms);

            var path = cpas.CombinedPlatformPath (level.P1, level.P2, level.Goal, level.Goal);

            string expected = "a, b, d, c, {X:105 Y:70}";

            Assert.AreEqual (expected, PlatformUtil.PlatListStr(path.Select(x => x.Item1)));
            Assert.AreEqual (expected, PlatformUtil.PlatListStr(path.Select(x => x.Item2)));
        }

    }
}

