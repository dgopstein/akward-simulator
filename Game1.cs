#region Using Statements
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;

#endregion

namespace awkwardsimulator
{
	// This is the main type for your game.
	public class Game1 : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		SpriteFont spriteFont;
		Texture2D whiteRectangle; // http://stackoverflow.com/questions/5751732/draw-rectangle-in-xna-using-spritebatch
		GameState state;
		ForwardModel fm;

		public Game1 ()
		{
			graphics = new GraphicsDeviceManager (this);
			Content.RootDirectory = "Content";	            
			graphics.IsFullScreen = true;		
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

			fm = new ForwardModel ();

			base.Initialize ();
		}

		// called once per game and is the place to load all of your content.
		protected override void LoadContent ()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch (GraphicsDevice);

			spriteFont = Content.Load<SpriteFont>("Default");

			// Create a 1px square rectangle texture that will be scaled to the
			// desired size and tinted the desired color at draw time
			whiteRectangle = new Texture2D(GraphicsDevice, 1, 1);
			whiteRectangle.SetData(new[] { Color.White });

		}

		protected override void UnloadContent()
		{
			base.UnloadContent();
			spriteBatch.Dispose();
			// If you are creating your texture (instead of loading it with
			// Content.Load) then you must Dispose of it
			whiteRectangle.Dispose();
		}
			
		// checking for collisions, gathering input, and playing audio.
		protected override void Update (GameTime gameTime)
		{
			KeyboardState keyState = Keyboard.GetState ();
			if (keyState.IsKeyDown(Keys.Escape)) { Exit (); }

			Tuple<Input, Input> inputs = ReadKeyboardInputs (keyState);


			state = fm.next (state, inputs.Item1, inputs.Item2);

			base.Update (gameTime);
		}
			
		/// This is called when the game should draw itself.
		protected override void Draw (GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear (Color.CornflowerBlue);
		
			//TODO: Add your drawing code here
			spriteBatch.Begin();
			spriteBatch.DrawString(spriteFont, "FPS: " + Math.Round(1000 / (gameTime.ElapsedGameTime.TotalMilliseconds + 1)), new Vector2(100, 100), Color.Red);

			DrawPlayer (state.p1);
			DrawPlayer (state.p2);
			
			spriteBatch.End();
            
			base.Draw (gameTime);
		}

		private Point Rasterize(Vector2 v) {
			return new Point (
				(int)Math.Round(v.X * GraphicsDevice.Viewport.Width),
				(int)Math.Round((1.0 - v.Y) * GraphicsDevice.Viewport.Height)
			);
		}

		private void DrawPlayer(Player p) {
			Point pt = Rasterize (p.Coords);
			Color c1, c2;

			if (p.Id == 1) {
				c1 = Color.DarkSlateGray;
				c2 = Color.LightGray;
			} else { //Id == 2
				c1 = Color.LightGray;
				c2 = Color.DarkSlateGray;
			}

			spriteBatch.Draw (whiteRectangle, new Rectangle (pt.X, pt.Y, 40, 60), c1);
			spriteBatch.DrawString(spriteFont, p.Id.ToString(), pt.ToVector2(), c2, 0, Vector2.Zero, 3, SpriteEffects.None, 0);
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

