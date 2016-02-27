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
		private World world;
        PlayerPhysics physP1, physP2;

		public World World { get { return world; } }

		private Shape rectShape(float width, float height) {			
			return new PolygonShape (PolygonTools.CreateRectangle(width/2, height/2, new Vector2(width/2, height/2), 0f), 1f);
		}

		private Fixture platformFix(float x, float y, float width = .3f, float height = .05f) {
			Body body = FarseerPhysics.Factories.BodyFactory.CreateBody(world, new Vector2(x, y));
			body.BodyType = BodyType.Static;

			return body.CreateFixture(rectShape (width, height));
		}

		private Fixture playerFix(float x, float y, float width = .01f, float height = .02f) {
			Body body = FarseerPhysics.Factories.BodyFactory.CreateBody(world, new Vector2(x, y));
			body.BodyType = BodyType.Dynamic;
			body.FixedRotation = true;
			body.Friction = 0.0f;

			return body.CreateFixture(rectShape (width, height));
		}

		public ForwardModel (GameState state)
		{
			world = new World (new Vector2 (0f, -100f));

            physP1 = new PlayerPhysics(playerFix (state.p1.X, state.p1.Y, state.p1.W, state.p1.H));
            physP2 = new PlayerPhysics(playerFix (state.p2.X, state.p2.Y, state.p2.W, state.p2.H));

            foreach (var plat in state.platforms) {
                platformFix (plat.X, plat.Y, plat.W, plat.H);
            }
		}

		public GameState next(GameState state, Input input1, Input input2) {
			GameState next = state.Clone();

            Debug.WriteLineIf ((Vector2.Distance (physP1.Fixture.Body.Position, state.p1.Coords) > 0.001f ||
                                Vector2.Distance (physP2.Fixture.Body.Position, state.p2.Coords) > 0.001f),
                "The gamestate and forwardmodel are out of sync"
            );

            // Calculate physics

			physP1.movePlayer (input1);
			physP2.movePlayer (input2);
			world.Step (1/30f); // XXX: pass in the right time-step

            // Assign values to GameState

            next.p1 = next.p1.WithPosition(physP1.Fixture.Body.Position);
            next.p2 = next.p2.WithPosition(physP2.Fixture.Body.Position);
            next.health = HealthControl.Health(next);

			return next;
		}
	}
}

