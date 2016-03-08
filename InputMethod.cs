using System;
using Microsoft.Xna.Framework.Input;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace awkwardsimulator
{
    abstract class InputMethod {
        public Input input1 = new Input(), input2 = new Input();

        public InputMethod(GameState state) {
        }   

        public Tuple <Input, Input> Inputs () {
            var inputs = _inputs ();

            input1 = inputs.Item1;
            input2 = inputs.Item2;

            return inputs;
        }

        abstract protected Tuple <Input, Input> _inputs ();
    }

    class HumanInput : InputMethod {
        public HumanInput(GameState state) : base(state) {
        }

        override protected Tuple<Input, Input> _inputs() {
            return ReadKeyboardInputs (Keyboard.GetState());
        }

        private Tuple<Input, Input> ReadKeyboardInputs(KeyboardState newKeyboardState) {
            bool left1, right1, up1, left2, right2, up2;
            left1 = right1 = up1 = left2 = right2 = up2 = false;

            if (newKeyboardState.IsKeyDown (Keys.A    )) { left1  = true; }
            if (newKeyboardState.IsKeyDown (Keys.D    )) { right1 = true; }
            if (newKeyboardState.IsKeyDown (Keys.W    )) { up1   = true; }

            if (newKeyboardState.IsKeyDown (Keys.Left )) { left2  = true; }
            if (newKeyboardState.IsKeyDown (Keys.Right)) { right2 = true; }
            if (newKeyboardState.IsKeyDown (Keys.Up   )) { up2    = true; }

            Input input1 = new Input (left1, right1, up1);
            Input input2 = new Input (left2, right2, up2);

            return Tuple.Create (input1, input2);
        }
    }

    abstract class AiInput : InputMethod {
        public AI ai1, ai2;


        public AiInput(GameState state) : base(state) {}

        abstract public void Update (GameState state);
    }

    class SingleAiInput : AiInput {
        protected Task<Input> fAi1, fAi2;

        public SingleAiInput(GameState state) : base(state) {
            startFAis(state);
        }

        protected void startFAis(GameState state) {
            fAi1 = Task.Factory.StartNew<Input> (() => ai1.nextInput (state));
            fAi2 = Task.Factory.StartNew<Input> (() => ai2.nextInput (state));
        }

        override protected Tuple <Input, Input> _inputs() {
            return Tuple.Create (fAi1.Result, fAi2.Result);
        }

        override public void Update(GameState state) {
            if (fAi1.IsCompleted && fAi1.IsCompleted) startFAis (state);
        }
    }

    class ListAiInput : AiInput {
        List<Input> inputQ1, inputQ2;
        protected Task<List<Input>> fAi1, fAi2;

        protected void startFAi1(GameState state) {
            fAi1 = Task.Factory.StartNew<List<Input>>(() => ai1.nextInputs (state));
        }
        protected void startFAi2(GameState state) {
            fAi2 = Task.Factory.StartNew<List<Input>> (() => ai2.nextInputs (state));
        }

        public ListAiInput(GameState state) : base(state) {
            inputQ1 = inputQ2 = new List<Input> (){ new Input () };

            //            ai1 = new WaypointAStar (state, PlayerId.P1);
            ai1 = new NullAI (state, PlayerId.P1);
//            ai2 = new WaypointAStar (state, PlayerId.P2, Heuristics.heuristic);
            ai2 = new AStar(state, PlayerId.P2, Heuristics.WaypointDistance);
            //            ai2 = new NullAI (state, PlayerId.P2);
            startFAi1 (state);
            startFAi2 (state);
        }

        override protected Tuple <Input, Input> _inputs() {
            Input input1, input2;

            if (fAi1.IsCompleted) inputQ1 = fAi1.Result;
            if (fAi2.IsCompleted) inputQ2 = fAi2.Result;

            if (inputQ1.Count == 0) { inputQ1 = fAi1.Result; }
            input1 = inputQ1.First ();
            inputQ1.RemoveAt (0);


            if (inputQ2.Count == 0) { inputQ2 = fAi2.Result; }
            input2 = inputQ2.First ();
            inputQ2.RemoveAt (0);

            return Tuple.Create (input1, input2);
        }

        override public void Update(GameState state) {
            if (fAi1.IsCompleted) startFAi1 (state);
            if (fAi2.IsCompleted) startFAi2 (state);
        }
    } 
}

