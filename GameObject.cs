﻿using System;

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

        public float LeftBoundary   { get { return X;     } }
        public float RightBoundary  { get { return X + W; } }
        public float TopBoundary    { get { return Y + H; } }
        public float BottomBoundary { get { return Y;     } }

        virtual public List<Vector2> Surface { get {
                return new List<Vector2> () {
                    TopLeft,
                    TopRight
            };
        } }

        virtual public List<Vector2> AccessSurface { get {
                return new List<Vector2> () {
                TopLeft + Player.Size * new Vector2 (-1, 1),
                TopRight + Player.Size
            };
        } }

        virtual public List<Vector2> ExitSurface { get {
                return new List<Vector2> () {
                    TopLeft  + new Vector2 (0.1f, 0),
                    TopRight + new Vector2(-0.1f, 0)
                };
            } }

        public Vector2 BottomLeft  { get { return new Vector2 (X    , Y    ); } }
        public Vector2 BottomRight { get { return new Vector2 (X + W, Y    ); } }
        public Vector2 TopRight    { get { return new Vector2 (X + W, Y + H); } }
        public Vector2 TopLeft     { get { return new Vector2 (X    , Y + H); } }

        virtual public List<Vector2> Corners { get {
            return new List<Vector2>() {
                        BottomLeft,
                        BottomRight,
                        TopRight,
                        TopLeft,
            };
        } }


        virtual public Vector2 Center { get { return Coords + (Vector2.Multiply (Size, 0.5f)); } }
        virtual public Vector2 SurfaceCenter { get { return Coords + (Vector2.Multiply (Size, new Vector2(0.5f, 1f))); } }
        virtual public Vector2 Target { get { return SurfaceCenter; } }

        public float Distance(GameObject b) {
            return Vector2.Distance (SurfaceCenter, b.SurfaceCenter);
        }

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

        public static new readonly Vector2 Size = new Vector2(4f, 6f);

        public Player (int id, Vector2 coords, Vector2 velocity = default(Vector2)) : base(coords, size: Size) {
            Id = id;
            this.Velocity = velocity;
        }

        // Player surface is on the bottom, because that's where it stands on platforms from
        override public Vector2 SurfaceCenter { get { return Coords + (Vector2.Multiply (Size, new Vector2(0.5f, 0f))); } }

        public Player Clone(Vector2 coords, Vector2 velocity) { return new Player(Id, coords, velocity); }
        public Player Clone(Vector2 coords) {  return Clone(coords, Velocity); }
        public Player Clone() {  return Clone(Coords); }


        public override string ToString () {
            return string.Format ("P{0}{1}", Id, base.ToString());
        }

        public override bool Equals (object obj) {
            if (obj == null) {
                return false;
            }

            Player p = obj as Player;
            if ((System.Object)p == null) {
                return false;
            }

            return (Coords == p.Coords) && (Velocity == p.Velocity) && (Id == p.Id);
        }

        // http://stackoverflow.com/questions/12393467/generate-hash-of-object-consistently
        public override int GetHashCode() {
            unchecked {
                int result = Coords.GetHashCode ();
                result = (result*397) ^ Velocity.GetHashCode();
                result = (result*397) ^ Id.GetHashCode();
                return result;
            }
        }
	}

	public class Platform : GameObject {
        private string name;
        public string Name { get { return name; } }

        public Platform (Vector2 coords, Vector2 size) : base (coords, size) {}
        public Platform (string name, Vector2 coords, Vector2 size) : base (coords, size) {
            this.name = name;
        }

        override public string ToString() {
            return name;
        }
	}

    abstract public class GameObjectCircle : GameObject {
        private float radius;
        public float Radius { get { return radius; } }

        public GameObjectCircle (Vector2 coords, float radius) : base (coords, size: new Vector2(radius, radius)) {
            this.radius = radius;
        }

        override public Vector2 Center { get { return Coords; } }
        override public Vector2 SurfaceCenter { get { return Coords; } }

        override public List<Vector2> Corners { get {
                return new List<Vector2> () {
                    new Vector2 (X - Radius, Y - Radius),
                    new Vector2 (X - Radius, Y + Radius),
                    new Vector2 (X + Radius, Y - Radius),
                    new Vector2 (X + Radius, Y + Radius),
                };
            } }
    }

    public class Goal : GameObjectCircle {
        public Goal (Vector2 coords, float radius) : base (coords, radius) { }
        override public Vector2 SurfaceCenter { get { return Coords - new Vector2(0, Radius); } }
        override public Vector2 Target { get { return Center; } }
        override public List<Vector2> Surface { get {
                return Corners;
        } }
        override public List<Vector2> AccessSurface { get {
                return Surface;
        } }
        override public List<Vector2> ExitSurface { get {
                return Surface;
        } }

    }
}

