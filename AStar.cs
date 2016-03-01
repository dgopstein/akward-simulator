using System;

namespace awkwardsimulator
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Priority_Queue;

    public class Leaf
    {
        public GameState game;
        public Input move;
        public float node_depth;

        public Leaf(GameState gamestate, Input rootmove, float nd) {
            game = gamestate;
            move = rootmove;
            node_depth = nd;
        }
    }

    public class AStar : AI {
        static private Input[] inputs = {new Input(false, true, false), new Input(true, false, false), new Input(false, false, true), new Input(true, false, true), 
            new Input(false, true, true), new Input(false, false, false)};
        //public int playerId;
        public SimplePriorityQueue<Leaf> leaves;
//        private float minDistance = 50;

        public AStar(GameState state, PlayerId pId) : base(state, pId) {}

        override public Input nextInput(GameState game) {
            return runAStar(game);
        }

        public Input runAStar(GameState game) {
            leaves = new SimplePriorityQueue<Leaf>();

            float score;
            GameState newGame;
            foreach (Input i in inputs) {
                newGame = this.next(game,i);
                score = this.heuristic(newGame);
                leaves.Enqueue(new Leaf(newGame, i, 1.0f), score + 1.0f);
            }       

            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedMilliseconds < 40 && leaves.Count > 0) {
                Leaf leaf = leaves.Dequeue();
                if (leaf.game.PlayStatus().isDied()) {
                  continue;
                }
                if (leaf.game.PlayStatus().isWon()) {
                    return leaf.move;
                }
                foreach (Input i in inputs) {
                    newGame = this.next(game, i);
                    newGame = this.next(newGame, i);

                    if (leaves.Count < 1000 && newGame.PlayStatus().isWon()) {
                        score = this.heuristic(newGame);
                        leaves.Enqueue(new Leaf(newGame, leaf.move, leaf.node_depth + 1.0f), score + leaf.node_depth + 1.0f);
                    }
                }
            }
            Leaf top = (leaves.Count > 0) ? leaves.Dequeue() : null;
            //System.Console.WriteLine("{0} : {1}" , playerId, top.node_depth);
            Input move = (leaves.Count > 0) ? top.move : new Input();

            return move;
        }
    }

}

