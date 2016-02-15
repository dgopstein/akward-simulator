using System;

using Microsoft.Xna.Framework;

namespace awkwardsimulator
{
	public class Player
	{
		public Vector2 Coords { get; set; }

		public float X { get { return Coords.X; } }
		public float Y { get { return Coords.Y; } }

		public int Id { get; }

		public Player (int id)
		{
			Id = id;

			Coords = new Vector2 (0, 0);
		}

		public Player Clone() {
			Player p = new Player(Id);
			p.Coords = this.Coords; // I think Vector2s are immutable
			return p;
		}
	}
}

