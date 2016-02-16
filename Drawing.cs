using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace awkwardsimulator
{
	public class Drawing
	{
		// Create a 1px square rectangle texture that will be scaled to the
		// desired size and tinted the desired color at draw time
		// http://stackoverflow.com/questions/5751732/draw-rectangle-in-xna-using-spritebatch
		// https://web.archive.org/web/20130718163159/http://www.xnawiki.com/index.php/Drawing_2D_lines_without_using_primitives
		private Texture2D blankTexture;
		public Texture2D BlankTexture { get { return blankTexture; } }

		private GraphicsDevice graphicsDevice;
		public GraphicsDevice GraphicsDevice { get { return graphicsDevice; } }

		private SpriteBatch spriteBatch;
		public SpriteBatch SpriteBatch { get { return spriteBatch; } }

		private SpriteFont spriteFont;
		public SpriteFont SpriteFont { get { return spriteFont; } }

		public Drawing(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont spriteFont) {
			this.graphicsDevice = graphicsDevice;
			this.spriteBatch = spriteBatch;
			this.spriteFont = spriteFont;

			blankTexture = new Texture2D(graphicsDevice, 1, 1);
			blankTexture.SetData(new[] { Color.White });
		}

		public Point Rasterize(Vector2 v) {
			return new Point (
				(int)Math.Round(v.X * graphicsDevice.Viewport.Width),
				(int)Math.Round((1.0 - v.Y) * graphicsDevice.Viewport.Height)
			);
		}

		public void DrawPlayer(Player p) {
			Point pt = Rasterize (p.Coords);
			Color c1, c2;

			if (p.Id == 1) {
				c1 = Color.DarkSlateGray;
				c2 = Color.LightGray;
			} else { //Id == 2
				c1 = Color.LightGray;
				c2 = Color.DarkSlateGray;
			}

			spriteBatch.Draw (blankTexture, new Rectangle (pt.X, pt.Y, 40, 60), c1);
			spriteBatch.DrawString(spriteFont, p.Id.ToString(), pt.ToVector2(), c2, 0, Vector2.Zero, 3, SpriteEffects.None, 0);
		}
	}
}

