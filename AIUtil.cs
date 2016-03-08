using System;

namespace awkwardsimulator
{
    public static class AIUtil
    {

        public static Player thisPlayer(this AI ai, GameState state) {
            return state.Player (ai.pId);
        }

        public static Player otherPlayer(this AI ai, GameState state) {
            return state.Player (ai.pId == PlayerId.P1 ? PlayerId.P2 : PlayerId.P1);
        }
    }
}

