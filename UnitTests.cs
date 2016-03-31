﻿using System;
using NUnit.Framework;

using PlatGraph = System.Collections.Generic.Dictionary<awkwardsimulator.Platform, System.Collections.Generic.HashSet<awkwardsimulator.Platform>>;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace awkwardsimulator
{
    [TestFixture]
    public class UnitTests
    {
        [Test()]
        public void PlatformGraphLevel1() {
            PlatGraph generated = PlatformUtil.BuildPlatformGraph (Level.Level1.Platforms);

            string expected = "a[a, b], b[a, b, d], c[c, d], d[b, c, d]";

            Assert.AreEqual (expected, PlatformUtil.PlatGraphStr(generated));
        }

        [Test()]
        public void PlatformAdjacency1() {
            var level = Level.Level1;

            var a = level.Platform ("a");
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
        public void PlatformAdjacency2() {
            var level = Level.Level2;

            //            var a = level.Platform ("a");
            GameObject b = level.Platform ("b");
            GameObject c = level.Platform ("c");
            GameObject d = level.Platform ("d");
            GameObject goal = level.Goal;

            var list = new List<Tuple<bool, GameObject, GameObject>> () {
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

            path = cpas.CombinedPlatformPath (
                new Player(1, new Vector2(90, 36)),
                new Player(2, new Vector2(90, 45)),
                level.Goal, level.Goal);
            expected = "d|c, c|c, {X:105 Y:70}|{X:105 Y:70}";
            Assert.AreEqual (expected, PlatformUtil.PlatPairListStr (path));
        }

        [Test()]
        public void CombinedPlatformPath2() {
            var level = Level.Level2;

            var cpas = new CombinedPlatformAStar (level.Platforms);

            var path = cpas.CombinedPlatformPath (level.P1, level.P2, level.Goal, level.Goal);
//            string expected = "b|c, d|e, f|g, h|a, {X:80 Y:80}|{X:80 Y:80}";
            string expected = "b|c, d|e, d|g, f|a, h|a, {X:80 Y:80}|{X:80 Y:80}";
            Assert.AreEqual (expected, PlatformUtil.PlatPairListStr (path));
        }

        [Test()]
        public void PlatsBelow() {
            var level = Level.Level1;

            var ab = PlatformUtil.PlatListStr(PlatformUtil.platsBelow (level.Platforms, level.P1));
            var expectedAB = "a, b";
            Assert.AreEqual (expectedAB, ab);

            var abcd = PlatformUtil.PlatListStr(PlatformUtil.platsBelow (
                level.Platforms, level.P1.Clone(new Microsoft.Xna.Framework.Vector2(54, 1000))));
            var expectedABCD = "a, b, c, d";
            Assert.AreEqual (expectedABCD, abcd);

            var ab2 = PlatformUtil.PlatListStr(PlatformUtil.platsBelow (
                level.Platforms, level.P1.Clone(new Microsoft.Xna.Framework.Vector2(53.92445f, 13.60124f))));
            var expectedAB2 = "";
            Assert.AreEqual (expectedAB2, ab2);
        }
    }
}

