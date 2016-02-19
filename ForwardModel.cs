using System;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;

namespace awkwardsimulator
{

	public class ForwardModel
	{
		private World world;
//		Fixture p1Fix, p2Fix;//, plat;
		PlayerPhysics p1, p2;

		public World World { get { return world; } }

		private Shape rectShape(float width, float height) {			
			return new PolygonShape (PolygonTools.CreateRectangle(width/2, height/2, new Vector2(width/2, height/2), 0f), 1f);
		}

		private Fixture playerFix(float x, float y, float width = .01f, float height = .02f) {
			Body body = FarseerPhysics.Factories.BodyFactory.CreateBody(world, new Vector2(x, y));
			body.BodyType = BodyType.Dynamic;
			body.FixedRotation = true;
			body.Friction = 0.0f;

			return body.CreateFixture(rectShape (width, height));
		}

		private Fixture platformFix(float x, float y, float width = .3f, float height = .05f) {
			Body body = FarseerPhysics.Factories.BodyFactory.CreateBody(world, new Vector2(x, y));
			body.BodyType = BodyType.Static;

			return body.CreateFixture(rectShape (width, height));
		}


		public ForwardModel ()
		{
			world = new World (new Vector2 (0f, -100f));

			p1 = new PlayerPhysics(playerFix (21f, 50f, 1f, 2f));
			p2 = new PlayerPhysics(playerFix (30f, 40f, 1f, 2f));
			/*plat =*/platformFix (10f, 20f, 70f, 5f);
		}

		public GameState next(GameState state, Input input1, Input input2) {
			GameState next = state.Clone();

//			movePlayer (p1Fix, input1);
//			movePlayer (p2Fix, input2);
			p1.movePlayer (input1);
			p2.movePlayer (input2);
			world.Step (1/30f); // XXX: pass in the right time-step


			next.p1.Coords = p1.Fixture.Body.Position;
			next.p2.Coords = p2.Fixture.Body.Position;
			return next;
		}

		private Vector2 velocity(Input i) {
			double xspeed = 15, yspeed = 1;
			double x = 0, y = 0;

			if (i.left ) { x -= xspeed; }
			if (i.right) { x += xspeed; }
			if (i.up   ) { y += yspeed; }

			return new Vector2 ((float)x, (float)y);
		}
	}
}

