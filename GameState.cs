using System;
using System.Collections.Generic;
using System.Linq;

namespace awkwardsimulator
{
    public abstract class PlayStatus {
        public bool isDied() { return GetType () == typeof(Died); }
        public bool isWon () { return GetType () == typeof(Won);  }
        public bool isPlaying () { return GetType () == typeof(Playing);  }
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
        public const int Width = 160;
        public const int Height = 100;

        private Player p1, p2;
        public Player P1 { get { return p1; } }
        public Player P2 { get { return p2; } }

        public Player Player(PlayerId pId) {
            Player player;

            switch (pId) {
            case PlayerId.P1: player = P1; break;
            case PlayerId.P2: player = P2; break;
            default: throw new Exception("Unknown player id!");
            }

            return player;
        }
		
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

        public Platform Platform(string name) {
            return Platforms.Find (p => p.Name == name);
        }

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
            return String.Format ("{0} {1} {2} {3}", P1, P2, Health, PlayStatus);
        }

        private bool inGoal(Player p) {
            return Util.euclideanDistance (p.Center, goal.Coords) <= goal.Radius;
        }

        public static bool IsGrounded(IEnumerable<Platform> platforms, Player player) {
            return platforms.Any (plat => {

                var horizontal =
                    plat.RightBoundary >= player.LeftBoundary &&
                    plat.LeftBoundary <= player.RightBoundary;

                var vertical = Math.Abs (player.BottomBoundary - plat.TopBoundary) < 0.1;

                return horizontal && vertical;
            });
        }

        public bool IsGrounded(Player player) {
            return IsGrounded (Platforms, player);
        }

        public PlayStatus PlayStatus { get {
                PlayStatus status;

                if (inGoal (p1) && inGoal (p2)) {
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
}

