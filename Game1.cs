﻿#region Using Statements
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
        List<Input> inputQ1, inputQ2;
        Task<List<Input>> fAi1, fAi2;
        protected void startFAi1(GameState state) {
            fAi1 = Task.Factory.StartNew<List<Input>>(() => ai1.nextInputs (state));
        }
        protected void startFAi2(GameState state) {
            fAi2 = Task.Factory.StartNew<List<Input>> (() => ai2.nextInputs (state));
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

            inputQ1 = inputQ2 = new List<Input> (){ new Input () };

//            ai1 = new WaypointAStar (state, PlayerId.P1);
            ai1 = new NullAI (state, PlayerId.P1);
            ai2 = new WaypointAStar (state, PlayerId.P2);
//            ai2 = new NullAI (state, PlayerId.P2);
            startFAi1 (state);
            startFAi2 (state);

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
            } else  {
                if (fAi1.IsCompleted) inputQ1 = fAi1.Result;
                if (fAi2.IsCompleted) inputQ2 = fAi2.Result;

                if (inputQ1.Count > 0) {
                    input1 = inputQ1.First ();
                    inputQ1.RemoveAt (0);
                } else {
                    inputQ1 = fAi1.Result;
                }

                if (inputQ2.Count > 0) {
                    input2 = inputQ2.First ();
                    inputQ2.RemoveAt (0);
                } else {
                    inputQ2 = fAi2.Result;
                }
            }

            state = forwardModel.nextState (state, input1, input2);

            if (fAi1.IsCompleted) startFAi1 (state);
            if (fAi2.IsCompleted) startFAi2 (state);

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

            var c1 = Color.Black;
            var c2 = Color.Black;
            drawing.DrawButtonArrow (state.P1, input1, c1);
            drawing.DrawButtonArrow (state.P2, input2, c2);

            drawing.DrawHealth (state.Health);

            drawing.DrawPlayStatus (state.PlayStatus);
            drawing.DrawHeuristic (ai1, state, 20, 50);
            drawing.DrawHeuristic (ai2, state, 20, 80);

            drawing.DrawPath (pas.PlatformPath(state.P1, state.Goal).Select (s => s.Center), Color.Maroon, 2);
            drawing.DrawCircle (2, ((WaypointAStar)ai2).NextWaypoint(state), Color.Crimson);

            drawing.DrawPath (history.Select (s => s.P1.Coords), Color.Thistle, 2);


            drawing.DrawPaths (ai1.BestPaths ().Select(p => p.Select(e => e.Item2.P1.Coords)));
            drawing.DrawPaths (ai2.BestPaths ().Select(p => p.Select(e => e.Item2.P2.Coords)));
			
			spriteBatch.End();
            			
//            drawing.DrawDebug ();

			base.Draw (gameTime);
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
}

