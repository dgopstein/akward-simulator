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

        override public string ToString() {
            return Coords.ToString ();
        }
	}

	public class Player : GameObject {
        public int Id { get; }
        public Vector2 Velocity { get; }

        public Vector2 Center { get { return Coords + (Vector2.Multiply (Size, 0.5f)); } }

        public Player (int id, Vector2 coords, Vector2 velocity = default(Vector2)) : base(coords, size: new Vector2(4f, 6f)) {
            Id = id;
            this.Velocity = velocity;
        }

        public Player Clone(Vector2 coords, Vector2 velocity) { return new Player(Id, coords, velocity); }
        public Player Clone(Vector2 coords) {  return Clone(coords, Velocity); }
        public Player Clone() {  return Clone(Coords); }


        public override string ToString ()
        {
            return string.Format ("P{0}{1}", Id, base.ToString());
        }
	}

	public class Platform : GameObject {
        public Platform (Vector2 coords, Vector2 size) : base (coords, size) {}
	}

    public class Goal : GameObject {
        private float radius;
        public float Radius { get { return radius; } }

        public Goal (Vector2 coords, float radius) : base (coords, size: new Vector2(radius, radius)) {
            this.radius = radius;
        }
    }
}

