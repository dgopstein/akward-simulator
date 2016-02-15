﻿#region Using Statements
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;

#endregion

namespace awkwardsimulator
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Game1 : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		SpriteFont spriteFont;
		Texture2D whiteRectangle; // http://stackoverflow.com/questions/5751732/draw-rectangle-in-xna-using-spritebatch
		GameState state;

		public Game1 ()
		{
			graphics = new GraphicsDeviceManager (this);
			Content.RootDirectory = "Content";	            
			graphics.IsFullScreen = true;		
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize ()
		{
			// TODO: Add your initialization logic here
			base.Initialize ();
				
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent ()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch (GraphicsDevice);

			spriteFont = Content.Load<SpriteFont>("Default");

			// Create a 1px square rectangle texture that will be scaled to the
			// desired size and tinted the desired color at draw time
			whiteRectangle = new Texture2D(GraphicsDevice, 1, 1);
			whiteRectangle.SetData(new[] { Color.White });

			state = new GameState();
			state.p1 = new Player (1);
			state.p1.Coords = new Vector2 (0.1f, 0.2f);
			state.p2 = new Player (2);
			state.p2.Coords = new Vector2 (0.2f, 0.2f);

			//TODO: use this.Content to load your game content here 
		}

		protected override void UnloadContent()
		{
			base.UnloadContent();
			spriteBatch.Dispose();
			// If you are creating your texture (instead of loading it with
			// Content.Load) then you must Dispose of it
			whiteRectangle.Dispose();
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update (GameTime gameTime)
		{
			// For Mobile devices, this logic will close the Game when the Back button is pressed
			// Exit() is obsolete on iOS
			#if !__IOS__
			if (GamePad.GetState (PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
			    Keyboard.GetState ().IsKeyDown (Keys.Escape)) {
				Exit ();
			}
			#endif

			Tuple<Input, Input> inputs = ReadKeyboardInputs (Keyboard.GetState ());

			ForwardModel fm = new ForwardModel ();
			state = fm.next (state, inputs.Item1, inputs.Item2);

			// TODO: Add your update logic here			
			base.Update (gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
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

		protected Point Rasterize(Vector2 v) {
			return new Point (
				(int)Math.Round(v.X * GraphicsDevice.Viewport.Width),
				(int)Math.Round((1.0 - v.Y) * GraphicsDevice.Viewport.Height)
			);
		}

		protected void DrawPlayer(Player p) {
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

		protected Tuple<Input, Input> ReadKeyboardInputs(KeyboardState newKeyboardState) {
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

