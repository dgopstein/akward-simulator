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
                new Platform("1", new Vector2(10f, 10f), new Vector2(40f, 5f)),
                new Platform("2", new Vector2(60f, 10f), new Vector2(30f, 5f)),
                new Platform("3", new Vector2(60f, 40f), new Vector2(30f, 5f)),
                new Platform("4", new Vector2(95f, 25f), new Vector2(30f, 5f)),
            }
        );

        public static readonly GameState Level2 = new GameState(
            health: 0f,

            p1: new Player (1, new Vector2 (70f, 15f)),
            p2: new Player (2, new Vector2 (85f, 15f)),

            goal: new Goal(new Vector2(80f, 80f), 10f),

            platforms: new List<Platform> {
                new Platform("1", new Vector2(80f, 10f), new Vector2(5f, 50f)),

                new Platform("2", new Vector2(40f, 10f), new Vector2(40f, 5f)),
                new Platform("3", new Vector2(80f, 10f), new Vector2(30f, 5f)),

                new Platform("4", new Vector2(60f, 25f), new Vector2(20f, 5f)),
                new Platform("5", new Vector2(95f, 25f), new Vector2(30f, 5f)),

                new Platform("6", new Vector2(40f, 40f), new Vector2(30f, 5f)),
                new Platform("7", new Vector2(80f, 40f), new Vector2(30f, 5f)),

                new Platform("8", new Vector2(60f, 55f), new Vector2(20f, 5f)),
                new Platform("9", new Vector2(95f, 55f), new Vector2(30f, 5f)),
            }
        );
    }
}

