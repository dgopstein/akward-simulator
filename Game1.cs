#region Using Statements
using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics.DebugView;
using System.Diagnostics;

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
		DebugViewXNA DebugView;
        AI ai1, ai2;

        static float GameWidth = 160f;
        static float GameHeight = 100f;
        Matrix proj = Matrix.CreateOrthographicOffCenter(0f, GameWidth, 0f, GameHeight, 0f, 1f);
        Matrix view = Matrix.Identity;

        public Game1 ()
		{
			graphics = new GraphicsDeviceManager (this);
			Content.RootDirectory = "Content";	            
//			graphics.IsFullScreen = true;
		}

		// load any non-graphic related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		protected override void Initialize ()
		{
			state = new GameState(
				p1: new Player (1, new Vector2 (20f, 50f)),
				p2: new Player (2, new Vector2 (30f, 40f)),
				health: 0f,
                platforms: new List<Platform> { new Platform(new Vector2(10f, 20f), new Vector2(70f, 5f)) },
                goal: new Goal(new Vector2(60f, 40f), 10f)
			);

			forwardModel = new ForwardModel (state);

            ai1 = new AStar (state, PlayerId.P1);
            ai2 = new AStar (state, PlayerId.P2);

			base.Initialize ();
		}

		// called once per game and is the place to load all of your content.
		protected override void LoadContent ()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch (GraphicsDevice);
			spriteFont = Content.Load<SpriteFont>("Default");

            drawing = new Drawing (GraphicsDevice, spriteBatch, spriteFont, new Vector2(GameWidth, GameHeight));
//			debugDraw = new DebugDraw (drawing, forwardModel.World);

			DebugView = new DebugViewXNA (forwardModel.World);
			DebugView.DefaultShapeColor = Color.White;
			DebugView.SleepingShapeColor = Color.LightGray;
			DebugView.LoadContent(GraphicsDevice, Content);
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
            Input input1 = inputs.Item1;
            Input input2 = inputs.Item2;

//            input1 = ai1.nextInput (state);
//            input2 = ai2.nextInput (state);
            Debug.WriteLine ("input1: {0}\n", input1);

//            state = forwardModel.next (state, input1, input2);
//            state = ForwardModel.Next (state, input1, input2);
            state = forwardModel.next (input1, input2);

			base.Update (gameTime);
		}
			
		/// This is called when the game should draw itself.
		protected override void Draw (GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear (Color.CornflowerBlue);
		
			//TODO: Add your drawing code here
			spriteBatch.Begin();

            drawing.DrawFPS (gameTime);
			drawing.DrawPlayer (state.P1);
			drawing.DrawPlayer (state.P2);

            foreach (var plat in state.Platforms) {
                drawing.DrawGameObjectRect (plat, Color.Beige);
            }

            drawing.DrawGameObjectCircle (state.Goal, Color.BurlyWood);

            drawing.DrawHealth (state.Health);
            drawing.DrawPlayStatus (state.PlayStatus());
			
			spriteBatch.End();
            			
			DebugView.RenderDebugData(ref proj, ref view); //XXX probably expensive
            
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

