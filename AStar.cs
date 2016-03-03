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

        public List<Tuple<Input, GameState>> ToPath() {
            Stack<Tuple<Input, GameState>> stack = new Stack<Tuple<Input, GameState>> ();

            StateNode node = this;
            while (node.Parent != null) {
                stack.Push (Tuple.Create (node.Input, node.State));
                node = node.Parent;
            }

            return stack.ToList ();
        }
    }

    public class AStar : AI
    {
        //SimplePriorityQueue<StateNode> pq = new SimplePriorityQueue<StateNode>();
        SortedDictionary<double, StateNode> allPaths;

        public AStar (GameState state, PlayerId pId) : base(state, pId)
        {
            allPaths = new SortedDictionary<double, StateNode> ();
        }

        override public List<List<Tuple<Input, GameState>>> BestPaths() {
            List<KeyValuePair<double,StateNode>> myPaths;
            lock(allPaths) {
                myPaths = allPaths.Take (5).ToList();
            }

            return myPaths
                .Select(sn => sn.Value.ToPath()).ToList();
        }

        public override Input nextInput (GameState origState)
        {
            int maxIters = 100;
            var paths = new SortedDictionary<double, StateNode>();

            int uniqueId = 1;
            Func<GameState, double> heuristic = (s) =>
                Math.Truncate(Heuristic.heuristic (this.thisPlayer (s), s) * 1000d) / 1000d + (0.0000001 * uniqueId++);

            var root = new StateNode (null, new Input(), origState);
            paths.Add(heuristic(origState), root);
            var best = root;

            for (int i = 0; i < maxIters && !best.State.PlayStatus.isWon() && paths.Count > 0; i++) {
                best.Children = Input.All.ToDictionary (input=>input, input=> new StateNode(best, input, nextState(best.State, input)));

                foreach (var c in best.Children) {
                    paths.Add(heuristic (c.Value.State), c.Value);
                }

                best = paths.First().Value;
                paths.Remove (paths.First().Key);
            }

            lock (allPaths) { allPaths = paths; }

            var res = best.FirstAncestor ().Input;
            return res;
        }
    }
}

