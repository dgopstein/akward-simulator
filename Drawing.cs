﻿using System;
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

        private Vector2 gameDimensions;

        public Drawing(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont spriteFont, Vector2 gameDimensions) {
			this.graphicsDevice = graphicsDevice;
			this.spriteBatch = spriteBatch;
			this.spriteFont = spriteFont;
            this.gameDimensions = gameDimensions;

			blankTexture = new Texture2D(graphicsDevice, 1, 1);
			blankTexture.SetData(new[] { Color.White });
		}

        public float xScale() { return graphicsDevice.Viewport.Width  / gameDimensions.X; }
        public float yScale() { return graphicsDevice.Viewport.Height  / gameDimensions.Y; }

        public int xScale(float x) { return (int)Math.Round (xScale () * x); }
        public int yScale(float y) { return (int)Math.Round (yScale () * y); }

        public Point RasterizeCoords(GameObject go) {
            return new Point (
                (int)Math.Round( xScale() * go.X),
                (int)Math.Round(-yScale() * (go.Y + go.H) +graphicsDevice.Viewport.Height)
			);
		}

        public Point RasterizeDims(GameObject go) {
            return new Point (xScale (go.W), yScale (go.H));
        }

		public void DrawPlayer(Player p) {
            Point pt = RasterizeCoords (p);
            Color color1, color2;

			if (p.Id == 1) {
				color1 = Color.DarkSlateGray;
				color2 = Color.LightGray;
			} else { //Id == 2
				color1 = Color.LightGray;
				color2 = Color.DarkSlateGray;
			}

            DrawGameObjectRect (p, color1);
			spriteBatch.DrawString(spriteFont, p.Id.ToString(), pt.ToVector2(), color2, 0, Vector2.Zero,
                                   scale: 2.0f, effects: SpriteEffects.None, layerDepth: 0);
		}

        public void DrawGameObjectRect(GameObject go, Color c) {
            Point pt = RasterizeCoords (go);
            Point dim = RasterizeDims (go);
            spriteBatch.Draw (blankTexture, new Rectangle (pt.X, pt.Y, dim.X, dim.Y), c);
        }
	}
}

