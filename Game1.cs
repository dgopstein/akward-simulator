#region Using Statements
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics.DebugView;

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
//		DebugDraw debugDraw;
		Drawing drawing;
		DebugViewXNA DebugView;

//		DebugViewXNA DebugView;
//		DebugView = new DebugViewXNA(World);
//		DebugView.RemoveFlags(DebugViewFlags.Shape);
//		DebugView.RemoveFlags(DebugViewFlags.Joint);
//		DebugView.DefaultShapeColor = Color.White;
//		DebugView.SleepingShapeColor = Color.LightGray;
//		DebugView.LoadContent(ScreenManager.GraphicsDevice, ScreenManager.Content);

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
				new Player (1, new Vector2 (0.1f, 0.2f)),
				new Player (2, new Vector2 (0.2f, 0.2f)),
				0f, false, false
			);

			forwardModel = new ForwardModel ();

			base.Initialize ();
		}

		// called once per game and is the place to load all of your content.
		protected override void LoadContent ()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch (GraphicsDevice);
			spriteFont = Content.Load<SpriteFont>("Default");

			drawing = new Drawing (GraphicsDevice, spriteBatch, spriteFont);
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


			state = forwardModel.next (state, inputs.Item1, inputs.Item2);

			base.Update (gameTime);
		}
			
		/// This is called when the game should draw itself.
		protected override void Draw (GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear (Color.CornflowerBlue);
		
			//TODO: Add your drawing code here
			spriteBatch.Begin();
			spriteBatch.DrawString(drawing.SpriteFont, "FPS: " + Math.Round(1000 / (gameTime.ElapsedGameTime.TotalMilliseconds + 1)), new Vector2(100, 100), Color.Red);

			drawing.DrawPlayer (state.p1);
			drawing.DrawPlayer (state.p2);
			
			spriteBatch.End();

			Matrix proj = Matrix.CreateOrthographicOffCenter(0f, 100f, 0f, 100f, 0f, 1f);
			Matrix view = Matrix.Identity;
			DebugView.RenderDebugData(ref proj, ref view);
            
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

