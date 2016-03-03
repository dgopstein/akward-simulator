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
//        public ForwardModel fm;
        public GameState state;
        public Input move;
        public float node_depth;

//        public Leaf(ForwardModel fm, Input rootmove, float nd) {
        public Leaf(GameState state, Input rootmove, float nd) {
//            this.fm = fm;
            this.state = state;
            move = rootmove;
            node_depth = nd;
        }
    }

    public class AStarPhilip : AI {
        static private Input[] inputs = {
            new Input(false, true, false), new Input(true, false, false), new Input(false, false, true),
            new Input(true, false, true), new Input(false, false, false), new Input(false, true, true)
        };

        public SimplePriorityQueue<Leaf> leaves;

        public AStarPhilip(GameState state, PlayerId pId) : base(state, pId) {}

        override public Input nextInput(GameState game) {
            return runAStar(game);
        }

        public Input runAStar(GameState game) {
            leaves = new SimplePriorityQueue<Leaf>();

            float score;
            GameState newGame;
            foreach (Input i in inputs) {
                newGame = this.nextState(game,i);
                score = this.heuristic(newGame);
                leaves.Enqueue(new Leaf(newGame, i, 1.0f), score + 1.0f);
            }       

            // iterations are processed at around 2/ms
            int count = 0;
            while (count < 100 && leaves.Count > 0) {
                count++;
                Leaf leaf = leaves.Dequeue();
                if (leaf.state.PlayStatus.isDied()) {
                  continue;
                }

                if (leaf.state.PlayStatus.isWon()) {
                    return leaf.move;
                }
                foreach (Input i in inputs) {
                    newGame = this.nextState(leaf.state, i);
//                    newGame = this.nextState(newGame, i);

                    if (leaves.Count < 10000) {
                        score = this.heuristic(newGame);
                        leaves.Enqueue(new Leaf(newGame, leaf.move, leaf.node_depth + 1.0f), score + leaf.node_depth + 1.0f);
                    }
                }
            }
            Leaf top = null;
            Input move = new Input();
            if (leaves.Count > 0) {
                top = leaves.Dequeue ();
//                Debug.WriteLine ("depth: {0}", top.node_depth);
                move = top.move;
            } else {
//                Debug.WriteLine("{0} : no nodes!" , thisPlayer(game).Id);
            }

            return move;
        }
    }

}

