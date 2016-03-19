using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace awkwardsimulator
{
    public class Level
    {
        public static readonly GameState Level1 = new GameState(
            health: 0f,

            p1: new Player (1, new Vector2 (20f, 15f)),
            p2: new Player (2, new Vector2 (30f, 15f)),

            goal: new Goal(new Vector2(105f, 70f), 10f),

            platforms: new List<Platform> {
                new Platform("a", new Vector2(10f, 10f), new Vector2(40f, 5f)),
                new Platform("b", new Vector2(60f, 10f), new Vector2(30f, 5f)),
                new Platform("c", new Vector2(60f, 40f), new Vector2(30f, 5f)),
                new Platform("d", new Vector2(95f, 25f), new Vector2(30f, 5f)),
            }
        );

        public static readonly GameState Level2 = new GameState(
            health: 0f,

            p1: new Player (1, new Vector2 (70f, 15f)),
            p2: new Player (2, new Vector2 (85f, 15f)),

            goal: new Goal(new Vector2(80f, 80f), 10f),

            platforms: new List<Platform> {
                new Platform("a", new Vector2(80f, 10f), new Vector2(5f, 50f)),

                new Platform("b", new Vector2(40f, 10f), new Vector2(40f, 5f)),
                new Platform("c", new Vector2(80f, 10f), new Vector2(30f, 5f)),

                new Platform("d", new Vector2(60f, 25f), new Vector2(20f, 5f)),
                new Platform("e", new Vector2(95f, 25f), new Vector2(30f, 5f)),

                new Platform("f", new Vector2(40f, 40f), new Vector2(30f, 5f)),
                new Platform("g", new Vector2(80f, 40f), new Vector2(30f, 5f)),

                new Platform("h", new Vector2(60f, 55f), new Vector2(20f, 5f)),
                new Platform("i", new Vector2(95f, 55f), new Vector2(30f, 5f)),
            }
        );
    }
}

