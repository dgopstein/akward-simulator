using System;
using Microsoft.Xna.Framework.Input;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;


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

        abstract public void Update (GameState state);
    }

    class HumanInput : InputMethod {
        InputMethod aiUpdater;

        public HumanInput(GameState state, InputMethod aiUpdater) : base(state) {
            this.aiUpdater = aiUpdater;
        }

        override protected Tuple<Input, Input> _inputs() {
            return ReadKeyboardInputs (Keyboard.GetState());
        }

        private static Tuple<Input, Input> ReadKeyboardInputs(KeyboardState newKeyboardState) {
            return Tuple.Create (ReadKeyboardInput(newKeyboardState, Keys.A,    Keys.D,     Keys.W ),
                                 ReadKeyboardInput(newKeyboardState, Keys.Left, Keys.Right, Keys.Up));
        }

        public static Input ReadKeyboardInput(KeyboardState newKeyboardState, Keys a, Keys d, Keys w) {
            return new Input (
                newKeyboardState.IsKeyDown (a),
                newKeyboardState.IsKeyDown (d),
                newKeyboardState.IsKeyDown (w));
        }

        override public void Update (GameState state) {
            aiUpdater.Update (state); // For path drawing
        }
    }

    abstract class AiInput : InputMethod {
        public AI ai1, ai2;


        public AiInput(AI ai1, AI ai2, GameState state) : base(state) {
            this.ai1 = ai1;
            this.ai2 = ai2;
        }
    }

    class SynchronizedAiInput : AiInput {
        protected Task<Input> fAi1, fAi2;

        public SynchronizedAiInput(AI ai1, AI ai2, GameState state) : base(ai1, ai2, state) {
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
            if (fAi1.IsCompleted && fAi2.IsCompleted) startFAis (state);
        }
    }

    class HalfHumanAiInput : AiInput {
        protected Task<Input> fAi1;

        public HalfHumanAiInput(AI ai1, GameState state) : base(ai1, null, state) {
            startFAis(state);
        }

        protected void startFAis(GameState state) {
            fAi1 = Task.Factory.StartNew<Input> (() => ai1.nextInput (state));
        }

        override protected Tuple <Input, Input> _inputs() {
            var keyboard = Keyboard.GetState ();

            return ai1.pId == PlayerId.P1 ?
                Tuple.Create (fAi1.Result, HumanInput.ReadKeyboardInput(keyboard, Keys.Left, Keys.Right, Keys.Up)) :
                Tuple.Create(HumanInput.ReadKeyboardInput(keyboard, Keys.Left, Keys.Right, Keys.Up), fAi1.Result);
        }

        override public void Update(GameState state) {
            if (fAi1.IsCompleted) startFAis (state);
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

        public ListAiInput(AI ai1, AI ai2, GameState state) : base(ai1, ai2, state) {
            inputQ1 = inputQ2 = new List<Input> (){ new Input () };

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

