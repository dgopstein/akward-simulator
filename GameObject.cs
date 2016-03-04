using System;

using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace awkwardsimulator
{

    public abstract class GameObject {
        public Vector2 Coords { get; }
        public Vector2 Size { get; }

		public float X { get { return Coords.X; } }
		public float Y { get { return Coords.Y; } }

        public float W { get { return Size.X; } }
        public float H { get { return Size.Y; } }

        virtual public List<Vector2> Corners() {
            return new List<Vector2>() {
                new Vector2(X    , Y    ),
                new Vector2(X + W, Y    ),
                new Vector2(X    , Y + H),
                new Vector2(X + W, Y + H)
            };
        }


        virtual public Vector2 Center { get { return Coords + (Vector2.Multiply (Size, 0.5f)); } }


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

        override public Vector2 Center { get { return Coords; } }

        override public List<Vector2> Corners() {
            return new List<Vector2> () {
                new Vector2 (X - Radius, Y - Radius),
                new Vector2 (X - Radius, Y + Radius),
                new Vector2 (X + Radius, Y - Radius),
                new Vector2 (X + Radius, Y + Radius),
            };
        }

        public Goal (Vector2 coords, float radius) : base (coords, size: new Vector2(radius, radius)) {
            this.radius = radius;
        }
    }
}

