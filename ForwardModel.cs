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

        private Fixture platformFix (float x, float y, float width, float height)
        {
            Body body = FarseerPhysics.Factories.BodyFactory.CreateBody (world, new Vector2 (x, y));
            body.BodyType = BodyType.Static;

            return body.CreateFixture (rectShape (width, height));
        }

        private Fixture playerFix (float x, float y, float width, float height)
        {
            Body body = FarseerPhysics.Factories.BodyFactory.CreateBody (world, new Vector2 (x, y));
            body.BodyType = BodyType.Dynamic;
            body.FixedRotation = true;
            body.Friction = 0.0f;

            return body.CreateFixture (rectShape (width, height));
        }

        private Fixture goalFix (float x, float y, float radius)
        {
            Body body = FarseerPhysics.Factories.BodyFactory.CreateBody (world, new Vector2 (x, y));
            body.BodyType = BodyType.Static;

            Fixture fix = body.CreateFixture (new CircleShape(radius, 1.0f));
            fix.CollidesWith = Category.None;

            return fix;
        }

        public ForwardModel (GameState state)
        {
            initialState = state;

            world = new World (new Vector2 (0f, -50f));

            physP1 = new StatelessPlayerPhysics (world, playerFix (state.P1.X, state.P1.Y, state.P1.W, state.P1.H));
            physP2 = new StatelessPlayerPhysics (world, playerFix (state.P2.X, state.P2.Y, state.P2.W, state.P2.H));

            foreach (var plat in state.Platforms) {
                platformFix (plat.X, plat.Y, plat.W, plat.H);
            }

            goalFix (state.Goal.X, state.Goal.Y, state.Goal.W);

            loadState (state);
        }

        private GameState nextState (float oldHealth, Input input1, Input input2)
        {
            // Calculate physics

            physP1.movePlayer (input1);
            physP2.movePlayer (input2);
            world.Step (1 / 20f); // XXX: pass in the right time-step

            Vector2 c1 = physP1.Fixture.Body.Position;
            Vector2 c2 = physP2.Fixture.Body.Position;
            Vector2 v1 = physP1.Fixture.Body.LinearVelocity;
            Vector2 v2 = physP2.Fixture.Body.LinearVelocity;

            // Assign values to GameState
            GameState next = initialState.Clone (
                                 p1: initialState.P1.Clone (c1, v1),
                                 p2: initialState.P2.Clone (c2, v2),
                                 health: HealthControl.Health (oldHealth, c1, c2)
                             );

            return next;
        }

        public GameState nextState (GameState state, Input input1, Input input2) {
            loadState (state);

            Debug.WriteLineIf ((Vector2.Distance (physP1.Fixture.Body.Position, state.P1.Coords) > 0.001f ||
                                Vector2.Distance (physP2.Fixture.Body.Position, state.P2.Coords) > 0.001f),
                "The gamestate and forwardmodel are out of sync"
            );

            return nextState (state.Health, input1, input2);
        }

        private void loadState(GameState state) {
            physP1.Fixture.Body.Position = state.P1.Coords;
            physP2.Fixture.Body.Position = state.P2.Coords;
            physP1.Fixture.Body.LinearVelocity = state.P1.Velocity;
            physP2.Fixture.Body.LinearVelocity = state.P2.Velocity;
        }

//        public static GameState Next (GameState state, Input input1, Input input2) {
//            ForwardModel fm = new ForwardModel (state);
//            return fm.next (input1, input2);
//        }
    }
}

