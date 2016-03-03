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

        abstract public Input nextInput(GameState state);
    }
}
