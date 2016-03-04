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
using System.Threading;

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
        List<GameState> history;
        PlatformAStar pas;

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

//            Debug.WriteLine (PlatformAStar.PlatGraphStr(PlatformAStar.PlatformGraph (state.Platforms)));

            pas = new PlatformAStar (state.Platforms);
            Debug.WriteLine (PlatformAStar.PlatListStr(pas.PlatformPath(state.P1, state.Goal)));

			forwardModel = new ForwardModel (state);

            history = new List<GameState> ();

            ai1 = new WaypointAStar (state, PlayerId.P1);
            ai2 = new NullAI (state, PlayerId.P2);
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

            if (true) {
                Tuple<Input, Input> inputs = ReadKeyboardInputs (keyState);
                input1 = inputs.Item1;
                input2 = inputs.Item2;
            } else if (aiWatch.ElapsedMilliseconds >= 0) {
                input1 = fAi1.Result;
                input2 = fAi2.Result;
            }

            state = forwardModel.nextState (state, input1, input2);


            if (fAi1.IsCompleted && fAi2.IsCompleted) {
                startAiFutures ();
            }

            history.Add (state);

			base.Update (gameTime);
		}
			
		/// This is called when the game should draw itself.
		protected override void Draw (GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear (Color.CornflowerBlue);
		
			spriteBatch.Begin();

            drawing.DrawFPS (gameTime);

            foreach (var plat in state.Platforms) {
                drawing.DrawGameObjectRect (plat, Color.Gainsboro);
            }

            drawing.DrawGameObjectCircle (state.Goal, Color.BurlyWood);

            drawing.DrawPlayer (state.P1);
            drawing.DrawPlayer (state.P2);

            drawing.DrawHealth (state.Health);

            drawing.DrawPlayStatus (state.PlayStatus);
            drawing.DrawHeuristic (ai1, state, 20, 50);
            drawing.DrawHeuristic (ai1, state, 20, 80);

            drawing.DrawPath (pas.PlatformPath(state.P1, state.Goal).Select (s => s.Center), Color.Maroon, 2);
            drawing.DrawCircle (2, ((WaypointAStar)ai1).NextWaypoint(state), Color.Crimson);

            drawing.DrawPath (history.Select (s => s.P1.Coords), Color.Thistle, 2);


            drawing.DrawPaths (ai1.BestPaths ().Select(p => p.Select(e => e.Item2.P1.Coords)));
            drawing.DrawPaths (ai2.BestPaths ().Select(p => p.Select(e => e.Item2.P2.Coords)));
			
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

