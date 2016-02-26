using System;

using Microsoft.Xna.Framework;

namespace awkwardsimulator
{

    public abstract class GameObject {
        public Vector2 Coords { get; }
        public Vector2 Size { get; }

		public float X { get { return Coords.X; } }
		public float Y { get { return Coords.Y; } }

        public float W { get { return Size.X; } }
        public float H { get { return Size.Y; } }

        public GameObject (Vector2 coords, Vector2 size) {
			this.Coords = coords;
            this.Size = size;
		}
	}

	public class Player : GameObject {
		public int Id { get; }
        public Player (int id, Vector2 coords) : base(coords, size: new Vector2(4f, 6f)) { Id = id; }
        public Player WithPosition(Vector2 coords) { return new Player(Id, coords); }
	}

	public class Platform : GameObject {
        public Platform (Vector2 coords, Vector2 size) : base (coords, size) {}
	}

    public class Goal : GameObject {
        public Goal (Vector2 coords) : base (coords, size: new Vector2(10f, 10f)) {}
    }
}

