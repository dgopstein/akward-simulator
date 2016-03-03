using System;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Common;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace awkwardsimulator
{
	public abstract class PlayerPhysics
	{
        protected Fixture fix;
        public Fixture Fixture { get { return fix; } }

        virtual protected bool Grounded { get; }

        public PlayerPhysics (Fixture fix) {
            this.fix = fix;
            fix.OnCollision = OnCollisionEventHandler;
            fix.OnSeparation = OnSeparationEventHandler;
        }

        abstract public void movePlayer (Input input, float dt = 0.2f);

        abstract protected bool OnCollisionEventHandler (Fixture fixtureA, Fixture fixtureB, Contact contact);
        abstract protected void OnSeparationEventHandler (Fixture fixtureA, Fixture fixtureB);
    }

    public class RealPlayerPhysics : PlayerPhysics {
		float JumpVelocity = 50.0f;
		float VariableJumpDampening = 0.75f; //Your jump speed is multiplied by this every frame unless you hold jump
		float GroundMoveAccel = 40.0f;
		float AirMoveAccel = 20.0f;
		float MaxMoveSpeed = 30.0f;
		float GroundFriction = 40.0f; //Applied to horizontal movement if neither key is held
		float AirFriction = 30.0f;

		bool holdingJumpButton = true;

        bool grounded = true;
        override protected bool Grounded { get { return grounded; } }

        public RealPlayerPhysics (Fixture fix) : base(fix) {}

		override public void movePlayer(Input input, float dt = 0.2f) {
			float mx = fix.Body.LinearVelocity.X;
			float my = fix.Body.LinearVelocity.Y;

			bool jumpButton = input.up;

            //TODO this will repeatedly jump
            if (jumpButton && Grounded) {
                my = JumpVelocity;
            } else if (!jumpButton && !Grounded && my > 0.0f) {
                my *= VariableJumpDampening;
            }
//			if (!jumpButton)
//				holdingJumpButton = false;
//
//			if (jumpButton && !holdingJumpButton && Grounded)
//			{
//				my = JumpVelocity;
//				holdingJumpButton = true;
//			}
//			else if (!Grounded)
//			{
//                if (!holdingJumpButton && my > 0.0f)	{
//					my *= VariableJumpDampening;
//				}
//			}

			float inputAxis = -1.0f * (input.left ? 1 : 0) + 1.0f * (input.right ? 1 : 0);
			float target = inputAxis * MaxMoveSpeed;
			float accel = (Grounded ? GroundMoveAccel : AirMoveAccel) * inputAxis * dt;

			bool hasInput = inputAxis != 0 ? true : false;
			bool isMoving = mx > 0.0f ? true : false;
			bool isAccelerating = hasInput && (!isMoving || (Math.Sign(target) == Math.Sign(mx) && Math.Abs(target) >= Math.Abs(mx)));
			bool isMovingBackward = hasInput && isMoving && Math.Sign(target) != Math.Sign(mx);
			bool frictionApplies = !hasInput || (!isAccelerating && !isMovingBackward);

			if (isAccelerating)
			{
				mx += accel;
			}
			else if (isMovingBackward)
			{
				mx += accel * 2.0f;
			}
			if (frictionApplies)
			{
				float drag = (Grounded ? GroundFriction : AirFriction) * dt;
				mx -= Math.Sign(mx) * Math.Min(Math.Abs(mx), drag);
			}
			float max = Math.Abs(target);
			mx = Util.clamp(mx, -max, max);

			fix.Body.LinearVelocity = new Vector2(mx, my);

		}

		Fixture ground = null;
		override protected bool OnCollisionEventHandler(Fixture fixtureA, Fixture fixtureB, Contact contact) {
			Vector2 normal;
			FixedArray2<Vector2> points;
			contact.GetWorldManifold(out normal, out points);

			if (contact.IsTouching && normal.Y < -.5f)  {
				ground = fixtureB;
				grounded = true;
			}

			return true;
		}

        override protected void OnSeparationEventHandler(Fixture fixtureA, Fixture fixtureB) {
			if (fixtureB == ground) {
				grounded = false;
			}
		}
	}

    public class StatelessPlayerPhysics : RealPlayerPhysics {
        private World world;

        public StatelessPlayerPhysics(World world, Fixture fix) : base(fix) {
            this.world = world;
            fix.OnCollision = null;
            fix.OnSeparation = null;
        }

        override protected bool Grounded {
            get {
                var offset = new Vector2 (0f, 0.1f);
                var pos = fix.Body.Position;
                var playerWidth = 4; // TODO get the real width
                var pts = new List<Vector2> { pos, new Vector2(pos.X + playerWidth, pos.Y) };
                var g = pts.Exists (pt => world.TestPoint (pt - offset) != null);

                return g;
            }
        }

        override protected bool OnCollisionEventHandler(Fixture fixtureA, Fixture fixtureB, Contact contact) { return true; }
        override protected void OnSeparationEventHandler (Fixture fixtureA, Fixture fixtureB) {}
    }
}

