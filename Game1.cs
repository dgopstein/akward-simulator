#region Using Statements
using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;

#endregion

namespace awkwardsimulator
{
	// This is the main type for your game.
	public class Game1 : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		SpriteFont spriteFont;
		GameState state;
		ForwardModel forwardModel;
		Drawing drawing;
        AI ai1, ai2;

        static float GameWidth = 160f;
        static float GameHeight = 100f;


        public Game1 ()
		{
			graphics = new GraphicsDeviceManager (this);
			Content.RootDirectory = "Content";	            
//			graphics.IsFullScreen = true;
		}

        Input input1, input2;
        Task<Input> fAi1, fAi2;
        Stopwatch aiWatch = new Stopwatch();
        protected void startAiFutures() {
            aiWatch.Reset ();
            aiWatch.Start ();
            fAi1 = Task.Factory.StartNew<Input>(() => ai1.nextInput (state));
            fAi2 = Task.Factory.StartNew<Input>(() => ai2.nextInput (state));
        }

		// load any non-graphic related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		protected override void Initialize ()
		{
            state = Level.Level1;

			forwardModel = new ForwardModel (state);

            ai1 = new AStar (state, PlayerId.P1);
            ai2 = new AStarPhilip (state, PlayerId.P2);
            startAiFutures ();

			base.Initialize ();
		}

		// called once per game and is the place to load all of your content.
		protected override void LoadContent ()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch (GraphicsDevice);
			spriteFont = Content.Load<SpriteFont>("Default");

            drawing = new Drawing (GraphicsDevice, spriteBatch, spriteFont,
                Content, forwardModel.World, new Vector2(GameWidth, GameHeight));
		}

		protected override void UnloadContent()
		{
			base.UnloadContent();
			spriteBatch.Dispose();
			// If you are creating your texture (instead of loading it with
			// Content.Load) then you must Dispose of it
			drawing.BlankTexture.Dispose();
		}
			
		// checking for collisions, gathering input, and playing audio.

		protected override void Update (GameTime gameTime)
		{
			KeyboardState keyState = Keyboard.GetState ();
			if (keyState.IsKeyDown(Keys.Escape)) { Exit (); }

			Tuple<Input, Input> inputs = ReadKeyboardInputs (keyState);
            input1 = inputs.Item1;
            input2 = inputs.Item2;

//            Task.WaitAll (new[] { fAi1, fAi2 });
            if (aiWatch.ElapsedMilliseconds >= 40) {
                input1 = fAi1.Result;
                input2 = fAi2.Result;

                state = forwardModel.nextState (state.Health, input1, input2);

                startAiFutures ();
            } else {
                state = forwardModel.nextState (state.Health, input1, input2);
            }
                

//            Debug.WriteLine ("input1: {0}\n", input1);

//            state = forwardModel.next (state, input1, input2);
//            state = ForwardModel.Next (state, input1, input2);


			base.Update (gameTime);
		}
			
		/// This is called when the game should draw itself.
		protected override void Draw (GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear (Color.CornflowerBlue);
		
			//TODO: Add your drawing code here
			spriteBatch.Begin();

            drawing.DrawFPS (gameTime);

            foreach (var plat in state.Platforms) {
                drawing.DrawGameObjectRect (plat, Color.Beige);
            }

            drawing.DrawGameObjectCircle (state.Goal, Color.BurlyWood);

            drawing.DrawPlayer (state.P1);
            drawing.DrawPlayer (state.P2);

            drawing.DrawHealth (state.Health);

            drawing.DrawPlayStatus (state.PlayStatus);
            drawing.DrawHeuristic (state.P1, state, 20, 50);
            drawing.DrawHeuristic (state.P2, state, 20, 80);

            drawing.DrawPaths (ai1.BestPaths ().Select(p => p.Select(e => e.Item2.P1.Coords)));
//            drawing.DrawPaths (ai2.BestPaths ().Select(p => p.Select(e => e.Item2.P2.Coords)));
			
			spriteBatch.End();
            			
//            drawing.DrawDebug ();

			base.Draw (gameTime);
		}

		private Tuple<Input, Input> ReadKeyboardInputs(KeyboardState newKeyboardState) {
			Input input1 = new Input(), input2 = new Input();

			if (newKeyboardState.IsKeyDown (Keys.A    )) { input1.left  = true; }
			if (newKeyboardState.IsKeyDown (Keys.D    )) { input1.right = true; }
			if (newKeyboardState.IsKeyDown (Keys.W    )) { input1.up    = true; }

			if (newKeyboardState.IsKeyDown (Keys.Left )) { input2.left  = true; }
			if (newKeyboardState.IsKeyDown (Keys.Right)) { input2.right = true; }
			if (newKeyboardState.IsKeyDown (Keys.Up   )) { input2.up    = true; }

			return Tuple.Create (input1, input2);
		}
	}
}

