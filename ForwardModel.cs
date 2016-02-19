﻿using System;
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
		Fixture p1Fix, p2Fix;//, plat;

		public World World { get { return world; } }

		private Shape rectShape(float width, float height) {			
			return new PolygonShape (PolygonTools.CreateRectangle(width/2, height/2, new Vector2(width/2, height/2), 0f), 1f);
		}

		private Fixture playerFix(float x, float y, float width = .01f, float height = .02f) {
			Body body = FarseerPhysics.Factories.BodyFactory.CreateBody(world, new Vector2(x, y));
			body.BodyType = BodyType.Dynamic;
			body.FixedRotation = true;

			return body.CreateFixture(rectShape (width, height));
		}

		private Fixture platformFix(float x, float y, float width = .3f, float height = .05f) {
			Body body = FarseerPhysics.Factories.BodyFactory.CreateBody(world, new Vector2(x, y));
			body.BodyType = BodyType.Static;

			return body.CreateFixture(rectShape (width, height));
		}

		private void movePlayer(Fixture fix, Input input) {
			//fix.Body.Position = fix.Body.Position + velocity(input);
			Vector2 lv = fix.Body.LinearVelocity;
			Vector2 delta = velocity (input);

			fix.Body.LinearVelocity = new Vector2 (delta.X, lv.Y);
			fix.Body.ApplyForce (new Vector2 (0f, delta.Y));
		}

		public ForwardModel ()
		{
			world = new World (new Vector2 (0f, -.8f));

			p1Fix = playerFix (.21f, .5f);
			p2Fix = playerFix (.2f, .4f);
			/*plat =*/platformFix (0.1f, 0.2f, 0.7f);
		}

		public GameState next(GameState state, Input input1, Input input2) {
			GameState next = state.Clone();

			movePlayer (p1Fix, input1);
			movePlayer (p2Fix, input2);
			world.Step (1/30f); // XXX: pass in the right time-step


			next.p1.Coords = p1Fix.Body.Position;
			next.p2.Coords = p2Fix.Body.Position;
			return next;
		}

		private Vector2 velocity(Input i) {
			double xspeed = 0.1, yspeed = 0.0005;
			double x = 0, y = 0;

			if (i.left ) { x -= xspeed; }
			if (i.right) { x += xspeed; }
			if (i.up   ) { y += yspeed; }

			return new Vector2 ((float)x, (float)y);
		}
	}
}

