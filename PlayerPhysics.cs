using System;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Common;

namespace awkwardsimulator
{
	public class PlayerPhysics
	{

		float JumpVelocity = 50.0f;
		float VariableJumpDampening = 0.75f; //Your jump speed is multiplied by this every frame unless you hold jump
		float GroundMoveAccel = 40.0f;
		float AirMoveAccel = 20.0f;
		float MaxMoveSpeed = 30.0f;
		float GroundFriction = 40.0f; //Applied to horizontal movement if neither key is held
		float AirFriction = 30.0f;

		bool grounded = true;
		bool boosting = false;
		bool holdingJumpButton = true;
		bool landed = true;
		bool wasGrounded = true;

		Fixture fix;

		public Fixture Fixture { get { return fix; } }

		public PlayerPhysics (Fixture fix)
		{
			this.fix = fix;
			fix.OnCollision = OnCollisionEventHandler;
			fix.OnSeparation = OnSeparationEventHandler;
		}

		public void movePlayer(Input input, float dt = 0.2f) {
			float mx = fix.Body.LinearVelocity.X;
			float my = fix.Body.LinearVelocity.Y;

			wasGrounded = grounded;
			grounded = landed;

			bool jumpButton = input.up;
			if (!jumpButton)
				holdingJumpButton = false;

			if (jumpButton && !holdingJumpButton && (grounded || wasGrounded))
			{
				my = JumpVelocity;
				boosting = true;
				holdingJumpButton = true;
			}
			else if (!grounded)
			{
				if (boosting && !jumpButton)
				{
					boosting = false;
				}
				//System.Console.WriteLine("(my: {0})", new object[]{my.ToString()});
				//if (!boosting && my > 0.0f)
				if (!boosting && my > 0.0f)
				{
					my *= VariableJumpDampening;
				}
			}

			float inputAxis = -1.0f * (input.left ? 1 : 0) + 1.0f * (input.right ? 1 : 0);
			float target = inputAxis * MaxMoveSpeed;
			float accel = (grounded ? GroundMoveAccel : AirMoveAccel) * inputAxis * dt;

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
				float drag = (grounded ? GroundFriction : AirFriction) * dt;
				mx -= Math.Sign(mx) * Math.Min(Math.Abs(mx), drag);
			}
			float max = Math.Abs(target);
			mx = Util.clamp(mx, -max, max);

			fix.Body.LinearVelocity = new Vector2(mx, my);

		}

		Fixture ground = null;
		bool OnCollisionEventHandler(Fixture fixtureA, Fixture fixtureB, Contact contact) {
			Vector2 normal;
			FixedArray2<Vector2> points;
			contact.GetWorldManifold(out normal, out points);

			if (contact.IsTouching && normal.Y < -.5f)  {
				ground = fixtureB;
				landed = true;
			}

			return true;
		}

		void OnSeparationEventHandler(Fixture fixtureA, Fixture fixtureB) {
			if (fixtureB == ground) {
				landed = false;
			}
		}
	}
}

