using System;

namespace awkwardsimulator
{
    public static class AIUtil
    {

        public static Player thisPlayer(this AI ai, GameState state) {
            Player player;

            switch (ai.pId) {
            case PlayerId.P1: player = state.P1; break;
            case PlayerId.P2: player = state.P2; break;
            default: throw new Exception("Unknown player id!");
            }

            return player;
        }

        public static Player otherPlayer(this AI ai, GameState world) {
            Player player;

            switch (ai.pId) {
            case PlayerId.P1: player = world.P1; break;
            case PlayerId.P2: player = world.P2; break;
            default: throw new Exception("Unknown player id!");
            }

            return player;
        }
    }
}

