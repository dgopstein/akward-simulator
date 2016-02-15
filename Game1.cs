#region Using Statements
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
		Player p1, p2;

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

			p1 = new Player (1);

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

			DrawPlayer (p1);
			
			spriteBatch.End();
            
			base.Draw (gameTime);
		}

		protected Point Scale(Vector2 v) {
			return new Point (
				(int)Math.Round(v.X * GraphicsDevice.Viewport.Width),
				(int)Math.Round(v.Y * GraphicsDevice.Viewport.Height)
			);
		}

		protected void DrawPlayer(Player p) {
			Point pt = Scale (p.Coords);
			spriteBatch.Draw(whiteRectangle, new Rectangle(pt.X, pt.Y, pt.X + 40, pt.Y + 60), new Color(127, 127, 127));
		}
	}
}

