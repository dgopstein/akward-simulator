using System;
using System.Collections.Generic;

namespace awkwardsimulator
{
    public abstract class Status {}
    public class Died : Status { public string cause { get; set; } }
    public class Playing : Status {}
    public class Won : Status {}

	public class GameState
	{
		public Player p1, p2;
		public float health;
        public Status status;
        public List<Platform> platforms;
        public Goal goal;

		public GameState (Player p1, Player p2, float health, Status status, List<Platform> platforms, Goal goal)
		{
			this.p1 = p1;
			this.p2 = p2;
			this.health = health;
			this.status = status;
			this.platforms = platforms;
			this.goal = goal;
		}

		public GameState(GameState s)
			: this(p1: s.p1, p2: s.p2, /*s.p1_previousInput.Clone(), s.p2_previousInput.Clone(), new StageAIData(s.stage),*/
                health: s.health, status: s.status, platforms: s.platforms, goal: s.goal)
		{ }

		public GameState Clone() {
			return new GameState(this);
		}
	}
}

