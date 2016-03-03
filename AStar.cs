using System;
using System.Collections.Generic;
using System.Linq;
//using Priority_Queue;
using System.Diagnostics;

namespace awkwardsimulator
{
    class StateNode {
        private StateNode parent;
        public  StateNode Parent { get { return parent; } }

        private Input input;
        public  Input Input { get { return input; } }

        private GameState state;
        public  GameState State { get { return state; } }

        private Dictionary<Input, StateNode> children;
        public  Dictionary<Input, StateNode> Children {
            get { return children;  }
            set { children = value; }
        }

        public StateNode(StateNode parent, Input input, GameState state) {
            this.parent = parent;
            this.input = input;
            this.state = state;

            this.children = new Dictionary<Input, StateNode> ();
        }

        public StateNode FirstAncestor () {
            StateNode node;

            Debug.WriteLine ("node: {0} {1}", Input, State.P1.Coords);

            if (parent == null) {
                Debug.WriteLine ("Returning the root!!!");
                node = this;
            } else if (parent.parent == null) {
                node = this;
            } else {
                node = parent.FirstAncestor ();
            }

            return node;
        }
    }

    public class AStar : AI
    {
        //SimplePriorityQueue<StateNode> pq = new SimplePriorityQueue<StateNode>();
        SortedDictionary<double, StateNode> allPaths;

        public AStar (GameState state, PlayerId pId) : base(state, pId)
        {
        }

        public override Input nextInput (GameState state)
        {
            allPaths = new SortedDictionary<double, StateNode>();

            Func<GameState, double> heuristic = (s) => Math.Truncate(Heuristic.heuristic (this.thisPlayer (s), s) * 1000d) / 1000d;
            int maxIters = 1000;

            var root = new StateNode (null, new Input(), state);
            allPaths.Add(heuristic(state), root);
            var best = root;

            for (int i = 0; i < maxIters && !best.State.PlayStatus.isWon() && allPaths.Count > 0; i++) {
                best.Children = Input.All.ToDictionary (input=>input, input=> new StateNode(best, input, nextState(state, input)));

                for (int j = 0; j < best.Children.Count; j++) {
                    var child = best.Children.ElementAt(j).Value;
                    var h = heuristic (child.State) + (0.0000001 * (i+1) + 0.00000001 * j);
                    allPaths.Add(h, child);
                }

                best = allPaths.First().Value;
                allPaths.Remove (allPaths.First().Key);

//                Debug.WriteLine(string.Join(", ", allPaths.Select(x => x.Key)));
//                Debug.WriteLine (allPaths.First ().Key);
            }

            Debug.WriteLine ("FirstAncestor");
            var res = best.FirstAncestor ().Input;
            return res;
        }
    }
}

