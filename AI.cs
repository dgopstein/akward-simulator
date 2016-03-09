using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using StateInput = System.Tuple<awkwardsimulator.GameState, awkwardsimulator.Input>;
//using Heuristic = System.Func<awkwardsimulator.GameState, awkwardsimulator.PlayerId, float>;

namespace awkwardsimulator
{
    public enum PlayerId { P1, P2 }

    abstract public class AI {
        public PlayerId pId;

        private ForwardModel forwardModel;

        private Heuristic heuristic;
        public Heuristic Heuristic { get { return heuristic; } }

        public AI(GameState state, PlayerId pId, Heuristic heuristic) {
            this.pId = pId;
            this.forwardModel = new ForwardModel(state);
            this.heuristic = heuristic;
        }

        virtual public List<Tuple<double, List<Tuple<Input, GameState>>>> BestPaths(int n) {
            return new List<Tuple<double, List<Tuple<Input, GameState>>>> ();
        }

        virtual public List<Tuple<double, List<Tuple<Input, GameState>>>> AllPaths() {
            return new List<Tuple<double, List<Tuple<Input, GameState>>>> ();
        }

        protected GameState nextState(GameState state, Input move) {
            return nextState(state, move, predictPartnerInput(state));
        }

        protected GameState nextState(GameState game, Input thisPlayerMove, Input otherPlayerMove) {
            GameState lastState = game;

            int intermediateSteps = 1; // set to match a reasonable Gameloop#humanInputDelayFrames() value...
            for (int i = 0; i < intermediateSteps; i++) {
                if (pId == PlayerId.P1) {
                    lastState = forwardModel.nextState(lastState, thisPlayerMove, otherPlayerMove);
                } else {
                    lastState = forwardModel.nextState(lastState, otherPlayerMove, thisPlayerMove);
                }
            }

            return lastState;
        }

        abstract protected Input predictPartnerInput (GameState state);

        abstract public List<Input> nextInputs(GameState state, PlayerId pId, Heuristic heuristic);

        public List<Input> nextInputs (GameState origState) {
            return nextInputs (origState, pId, heuristic);
        }

        public Input nextInput(GameState origState) {
            return nextInputs (origState).First ();
        }
    }

    public class NullAI : AI {
        public NullAI(GameState state, PlayerId pId) : base(state, pId, new LinearHeuristic(pId)) { }

        override public List<Input> nextInputs(GameState state, PlayerId pId, Heuristic heuristic) {
            return new List<Input>() { new Input () };
        }

        override protected Input predictPartnerInput(GameState state) {
            return new Input ();
        }
    }
}
