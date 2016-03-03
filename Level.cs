using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace awkwardsimulator
{
    public class Level
    {
        public static GameState Level1 = new GameState(
            health: 0f,

            p1: new Player (1, new Vector2 (20f, 10f)),
            p2: new Player (2, new Vector2 (30f, 10f)),

            goal: new Goal(new Vector2(105f, 70f), 10f),

            platforms: new List<Platform> {
                new Platform(new Vector2(10f, 10f), new Vector2(40f, 5f)),
                new Platform(new Vector2(60f, 10f), new Vector2(30f, 5f)),
                new Platform(new Vector2(60f, 40f), new Vector2(30f, 5f)),
                new Platform(new Vector2(95f, 25f), new Vector2(30f, 5f)),
            }
        );
    }
}

