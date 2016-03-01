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

        public Player thisPlayer(GameState state) {
            Player player;

            switch (pId) {
            case PlayerId.P1: player = state.P1; break;
            case PlayerId.P2: player = state.P2; break;
            default: throw new Exception("Unknown player id!");
            }

            return player;
        }

        public Player otherPlayer(GameState world) {
            Player player;

            switch (pId) {
            case PlayerId.P1: player = world.P1; break;
            case PlayerId.P2: player = world.P2; break;
            default: throw new Exception("Unknown player id!");
            }

            return player;
        }

        protected GameState next(GameState game, Input move) {
            return next(game, move, move); //TODO don't just copy this-player's move
        }

        protected GameState next(GameState game, Input thisPlayerMove, Input otherPlayerMove) {
            GameState lastState = game;

            int intermediateSteps = 3; // set to match a reasonable Gameloop#humanInputDelayFrames() value...
            for (int i = 0; i < intermediateSteps; i++) {
                if (pId == PlayerId.P1) {
                    lastState = ForwardModel.next(lastState, thisPlayerMove, otherPlayerMove);
                } else {
                    lastState = ForwardModel.next(lastState, otherPlayerMove, thisPlayerMove);
                }
            }

            return lastState;
        }

        abstract public Input nextInput(GameState state);
    }
}
