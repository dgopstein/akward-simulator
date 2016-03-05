using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using StateInput = System.Tuple<awkwardsimulator.GameState, awkwardsimulator.Input>;
using SMath = System.Math;

namespace awkwardsimulator
{
    public enum PlayerId { P1, P2 }

    abstract public class AI {
        public PlayerId pId;

        private ForwardModel forwardModel;

        public AI(GameState state, PlayerId pId) {
            this.pId = pId;
            this.forwardModel = new ForwardModel(state);
        }

        abstract public float Heuristic (GameState state);

        virtual public List<List<Tuple<Input, GameState>>> BestPaths() {
            return new List<List<Tuple<Input, GameState>>> ();
        }

        protected GameState nextState(GameState game, Input move) {
            return nextState(game, move, move); //TODO don't just copy this-player's move
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

        abstract public List<Input> nextInputs(GameState state);

        public Input nextInput(GameState origState) {
            return nextInputs (origState).First ();
        }
    }

    public class NullAI : AI {
        public NullAI(GameState state, PlayerId pId) : base(state, pId) { }

        override public float Heuristic(GameState state) {
            return 0f;
        }

        override public List<Input> nextInputs(GameState state) {
            return new List<Input>() { new Input () };
        }
    }
}
