using System;

namespace awkwardsimulator
{
	public class GameState
	{
		public Player p1, p2;
		//public Input lastInput1, lastInput2;
		public float health;
		public bool win, lose;

		public GameState (Player p1, Player p2, float health, bool win, bool lose)
		{
			this.p1 = p1;
			this.p2 = p2;
			this.health = health;
			this.win = win;
			this.lose = lose;
		}

		public GameState(GameState s)
			: this(s.p1.Clone(), s.p2.Clone(), /*s.p1_previousInput.Clone(), s.p2_previousInput.Clone(), new StageAIData(s.stage),*/
				s.health, s.win, s.lose)
		{ }

		public GameState Clone() {
			return new GameState(this);
		}
	}
}

