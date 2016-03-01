using System;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using System.Collections.Generic;
using System.Diagnostics;

namespace awkwardsimulator
{

    public class ForwardModel
    {
        private GameState initialState;
        private World world;
        private PlayerPhysics physP1, physP2;

        public World World { get { return world; } }

        private Shape rectShape (float width, float height)
        {			
            return new PolygonShape (PolygonTools.CreateRectangle (width / 2, height / 2, new Vector2 (width / 2, height / 2), 0f), 1f);
        }

        private Fixture platformFix (float x, float y, float width = .3f, float height = .05f)
        {
            Body body = FarseerPhysics.Factories.BodyFactory.CreateBody (world, new Vector2 (x, y));
            body.BodyType = BodyType.Static;

            return body.CreateFixture (rectShape (width, height));
        }

        private Fixture playerFix (float x, float y, float width = .01f, float height = .02f)
        {
            Body body = FarseerPhysics.Factories.BodyFactory.CreateBody (world, new Vector2 (x, y));
            body.BodyType = BodyType.Dynamic;
            body.FixedRotation = true;
            body.Friction = 0.0f;

            return body.CreateFixture (rectShape (width, height));
        }

        public ForwardModel (GameState state)
        {
            initialState = state;

            world = new World (new Vector2 (0f, -100f));

            physP1 = new PlayerPhysics (playerFix (state.P1.X, state.P1.Y, state.P1.W, state.P1.H));
            physP2 = new PlayerPhysics (playerFix (state.P2.X, state.P2.Y, state.P2.W, state.P2.H));

            foreach (var plat in state.Platforms) {
                platformFix (plat.X, plat.Y, plat.W, plat.H);
            }
        }

        public GameState next (Input input1, Input input2)
        {
            GameState state = initialState;

//            Debug.WriteLineIf ((Vector2.Distance (physP1.Fixture.Body.Position, state.P1.Coords) > 0.001f ||
//            Vector2.Distance (physP2.Fixture.Body.Position, state.P2.Coords) > 0.001f),
//                "The gamestate and forwardmodel are out of sync"
//            );

            // Calculate physics

            physP1.movePlayer (input1);
            physP2.movePlayer (input2);
            world.Step (1 / 30f); // XXX: pass in the right time-step

            Vector2 c1 = physP1.Fixture.Body.Position;
            Vector2 c2 = physP2.Fixture.Body.Position;

            // Assign values to GameState
            GameState next = state.Clone (
                                 p1: state.P1.WithPosition (c1),
                                 p2: state.P2.WithPosition (c2),
                                 health: HealthControl.Health (c1, c2)
                             );

            return next;
        }

        public static GameState next (GameState state, Input input1, Input input2) {
            ForwardModel fm = new ForwardModel (state);
            return fm.next (input1, input2);
        }
    }
}

