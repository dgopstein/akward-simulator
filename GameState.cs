using System;

namespace awkwardsimulator
{
	public class GameState
	{
		public Player p1, p2;
		//public Input lastInput1, lastInput2;
		public float health;
		public bool win, lose;

		public GameState ()
		{
		}

		public GameState(GameState s)
			//: this(s.p1.Clone(), s.p2.Clone(), s.p1_previousInput.Clone(), s.p2_previousInput.Clone(), new StageAIData(s.stage), s.health, s.win, s.lose) { }
		{
			p1 = s.p1.Clone();
			p2 = s.p2.Clone();
			health = s.health;
			win = s.win;
			lose = s.lose;
		}

		public GameState Clone() {
			return new GameState(this);
		}
	}
}

