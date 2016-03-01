using System;
using System.Collections.Generic;

namespace awkwardsimulator
{
    public abstract class PlayStatus {}
    public class Died : PlayStatus { public string cause { get; set; } }
    public class Playing : PlayStatus {}
    public class Won : PlayStatus {}

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
		public Player p1, p2;
		public float health;
        public PlayStatus playStatus;
        public List<Platform> platforms;
        public Goal goal;

		public GameState (Player p1, Player p2, float health, PlayStatus status, List<Platform> platforms, Goal goal)
		{
			this.p1 = p1;
			this.p2 = p2;
			this.health = health;
			this.playStatus = status;
			this.platforms = platforms;
			this.goal = goal;
		}

		public GameState(GameState s)
			: this(p1: s.p1, p2: s.p2, /*s.p1_previousInput.Clone(), s.p2_previousInput.Clone(), new StageAIData(s.stage),*/
                health: s.health, status: s.playStatus, platforms: s.platforms, goal: s.goal)
		{ }

		public GameState Clone() {
			return new GameState(this);
		}
	}
}

