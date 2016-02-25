using System;

using Microsoft.Xna.Framework;

namespace awkwardsimulator
{

    public abstract class GameObject<T> where T : GameObject<T> {
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

//        public virtual T Clone () {
//            return new T { Coords = this.Coords, Size = this.Size };
//		}

//        public abstract T Clone ()
	}

	public class Player : GameObject<Player> {
		public int Id { get; }
        public Player (int id, Vector2 coords) : base(coords, size: new Vector2(1f, 2f)) { Id = id; }
//		override public Player Clone() { return new Player(Id, Coords);	}
        public Player WithPosition(Vector2 coords) { return new Player(Id, coords); }
	}

	public class Platform : GameObject<Platform> {
        public Platform (Vector2 coords, Vector2 size) : base (coords, size) {}
	}

    public class Goal : GameObject<Goal> {
        public Goal (Vector2 coords) : base (coords, size: new Vector2(10f, 10f)) {}
    }
}

