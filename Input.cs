using System;
using System.Collections.Generic;

namespace awkwardsimulator
{
	public class Input {
		public readonly bool left;
        public readonly bool right;
        public readonly bool up;

        public readonly int ToInt;

        public Input() : this(false, false, false) {}

		public Input(bool left, bool right, bool up) {
			this.left  = left;
			this.right = right;
			this.up    = up;

            ToInt = InputToInt (left, right, up);
		}

		override public string ToString() {
            return mediumString ();
		}

        public string longString() {
            return String.Format("Input( {0} , {1} , {2} )", left, right, up);
        }

		public string mediumString() {
			return String.Format("{0}{1}{2}", left ? "L" : "_", right ? "R" : "_", up ? "U" : "_");
		}

		public string shortString() {
			if      ( left &&  right &&  up) { return "X"; }
			else if ( left &&  right && !up) { return "-"; }
			else if ( left && !right &&  up) { return "\\";}
			else if ( left && !right && !up) { return "<"; }
			else if (!left &&  right &&  up) { return "/"; }
			else if (!left &&  right && !up) { return ">"; }
			else if (!left && !right &&  up) { return "^"; }
			else /* (!left && !right && !up)*/{return "0"; }
		}

		//Override Reference Equals
		public static bool operator ==(Input a, Input b)
		{
			// If both are null, or both are same instance, return true.
			if (System.Object.ReferenceEquals(a, b))
			{
				return true;
			}

			// If one is null, but not both, return false.
			if (((object)a == null) || ((object)b == null))
			{
				return false;
			}

			// Return true if the fields match:
			return a.left == b.left && a.right == b.right && a.up == b.up;
		}

		public static bool operator !=(Input a, Input b)
		{
			return !(a == b);
		}

		//Override Equals
		public override bool Equals(System.Object obj)
		{
			// If parameter is null return false.
			if (obj == null)
			{
				return false;
			}

			// If parameter cannot be cast to Point return false.
			Input p = obj as Input;
			if ((System.Object)p == null)
			{
				return false;
			}

			// Return true if the fields match:
			return (left == p.left) && (right == p.right) && (up == p.up);
		}

		public bool Equals(Input p)
		{
			// If parameter is null return false:
			if ((object)p == null)
			{
				return false;
			}

			// Return true if the fields match:
			return (left == p.left) && (right == p.right) && (up == p.up);
		}

		public override int GetHashCode()
		{
			return left.GetHashCode() ^ right.GetHashCode() ^ up.GetHashCode();
		}

        private static int InputToInt(bool left, bool right, bool up) {
            Func<bool, int> b2i = b => b ? 1 : 0;

            return b2i (left) << 2 |
                b2i (right) << 1 |
                b2i (up);
        }

		public Input Clone() {
			return new Input(left, right, up);
		}

        public static Input Noop = new Input (false, false, false);
        public static Input Up = new Input (false, false, true);
        public static Input Right = new Input(false,  true, false);
        public static Input UpRight = new Input(false,  true,  true);
        public static Input Left = new Input(true,  false, false);
        public static Input UpLeft = new Input (true, false, true);

        public static List<Input> All = new List<Input> () {
            Noop, Up, Right, UpRight, Left, UpLeft
        };
	}
}

