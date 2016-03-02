using System;
using System.Collections.Generic;

namespace awkwardsimulator
{
    public abstract class PlayStatus {
        public bool isDied() { return GetType () == typeof(Died); }
        public bool isWon () { return GetType () == typeof(Won);  }
    }
    public class Died : PlayStatus {
        public string cause { get; set; }
        override public string ToString() { return cause; } 
    }
    public class Playing : PlayStatus { override public string ToString() { return "Playing"; } }
    public class Won : PlayStatus { override public string ToString() { return "Won"; } }

    public class Stage {
        public List<Platform> Platforms { get; }
        public Goal Goal { get; }

        public Stage(Goal goal, List<Platform> platforms) {
            Goal = goal;
            Platforms = platforms;
        }
    }

	public class GameState
	{
        private Player p1, p2;
        public Player P1 { get { return p1; } }
        public Player P2 { get { return p2; } }
		
        private float health;
        public float Health { get { return health; } }

        private List<Platform> platforms;
        public List<Platform> Platforms { get { return platforms; } }

        private Goal goal;
        public Goal Goal { get { return goal; } }

		public GameState (Player p1, Player p2, float health, List<Platform> platforms, Goal goal)
		{
			this.p1 = p1;
			this.p2 = p2;
			this.health = health;
			this.platforms = platforms;
			this.goal = goal;
		}

		public GameState(GameState s)
			: this(p1: s.P1, p2: s.P2, /*s.p1_previousInput.Clone(), s.p2_previousInput.Clone(), new StageAIData(s.stage),*/
                health: s.Health, platforms: s.platforms, goal: s.Goal)
		{ }

		public GameState Clone(
            Player p1 = null,
            Player p2 = null,
            float health = float.NegativeInfinity,
            PlayStatus status = null,
            List<Platform> platforms = null,
            Goal goal = null
        ) {
			return new GameState(
                p1: p1 == null ? this.p1 : p1,
                p2: p2 == null ? this.p2 : p2,
                health: health == float.NegativeInfinity ? this.Health : health,
                platforms: platforms == null ? this.platforms : platforms,
                goal: goal == null ? this.goal : goal
            );
		}

        override public string ToString() {
            return String.Format ("{0} {1} {2} {3}", P1, P2, Health, PlayStatus());
        }

        private bool inGoal(Player p) {
            return Util.euclideanDistance (p.Center, goal.Coords) <= goal.Radius;
        }

        public PlayStatus PlayStatus() {
            PlayStatus status;

            if (inGoal(p1) && inGoal(p2)) {
                status = new Won ();
            } else if (Health >= 1) {
                status = new Died { cause = "awkwardness" };
            } else if (Health <= -1) {
                status = new Died { cause = "loneliness" };
            } else if (P1.Y < 0) {
                status = new Died { cause = "p1 fell" };
            } else if (P2.Y < 0) {
                status = new Died { cause = "p2 fell" };
            } else {
                status = new Playing ();
            }

            return status;
        }
	}
}

