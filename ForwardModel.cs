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
		Fixture p1Fix, p2Fix, plat;

		public World World { get { return world; } }

		private Shape rectShape(float width, float height) {
			Vertices vs = new FarseerPhysics.Common.Vertices () {
				new Vector2 (-width/2, -height/2),
				new Vector2 (-width/2,  height/2),
				new Vector2 ( width/2, -height/2),
				new Vector2 ( width/2,  height/2)
			};

			return new PolygonShape(vs, 1f);
		}

		private Fixture playerFix(float x, float y) {
			Body body = FarseerPhysics.Factories.BodyFactory.CreateBody(world, new Vector2(x, y));
			body.BodyType = BodyType.Dynamic;

			return body.CreateFixture(rectShape (.01f, .02f));
		}

		private Fixture platformFix(float x, float y) {
			Body body = FarseerPhysics.Factories.BodyFactory.CreateBody(world, new Vector2(x, y));
			body.BodyType = BodyType.Static;

			return body.CreateFixture(rectShape (5f, .1f));
		}

		public ForwardModel ()
		{
			world = new World (new Vector2 (0f, -.8f));

			p1Fix = playerFix (.2f, 4f);
			p2Fix = playerFix (.04f, .1f);
			plat = platformFix (0f, 0.05f);
		}

		public GameState next(GameState state, Input input1, Input input2) {
			GameState next = state.Clone();

			world.Step (1/30f); // XXX: pass in the right time-step

			next.p1.Coords = p1Fix.Body.Position;
			next.p2.Coords = p2Fix.Body.Position;

			Console.WriteLine ("b p1: {0}", next.p1.Coords);
			Console.WriteLine ("plat: {0}", plat.Body.Position);

			return next;
		}

		protected Vector2 velocity(Input i) {
			double xspeed = 0.01, yspeed = 0.01;
			double x = 0, y = 0;

			if (i.left ) { x -= xspeed; }
			if (i.right) { x += xspeed; }
			if (i.up   ) { y += yspeed; }

			return new Vector2 ((float)x, (float)y);
		}
	}
}

