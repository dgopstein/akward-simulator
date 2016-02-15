using System;
using Microsoft.Xna.Framework;

namespace awkwardsimulator
{
	public class ForwardModel
	{
		public ForwardModel ()
		{
			
		}

		public GameState next(GameState state, Input input1, Input input2) {
			GameState next = state.Clone();

			next.p1.Coords =
				next.p1.Coords +
				velocity (input1);
			next.p2.Coords = next.p2.Coords + velocity (input2);

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

