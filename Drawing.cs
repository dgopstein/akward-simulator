using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using FarseerPhysics.Samples.DrawingSystem;
using System.Collections.Generic;


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

        public void DrawFPS(GameTime gameTime) {
            spriteBatch.DrawString(SpriteFont,
                "FPS: " + Math.Round(1000 / (gameTime.ElapsedGameTime.TotalMilliseconds + 1)),
                new Vector2(graphicsDevice.Viewport.Width - 100, 20), Color.Red);
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

        public void DrawHealth(float health) {
            float screenWidth = graphicsDevice.Viewport.Width;
            float screenHeight = graphicsDevice.Viewport.Height;

            int y = (int) (0.04 * screenHeight);
            int h = (int) (.05 * screenHeight);

            double totalW = 0.6 * screenWidth;

            int leftX = (int) (0.2 * screenWidth);
            int leftW = (int) (totalW * ((health + 1) / 2));

            int rightX = leftX + leftW;
            int rightW = (int) (totalW - leftW);

            int centerX = (int)(leftX + totalW / 2);

            spriteBatch.Draw (blankTexture, new Rectangle (leftX,  y, leftW,  h), Color.Coral);
            spriteBatch.Draw (blankTexture, new Rectangle (rightX, y, rightW, h), Color.Cornsilk);
            spriteBatch.DrawString(spriteFont, health.ToString("0.00"), new Vector2(centerX - 40, y), Color.Black, 0, Vector2.Zero,
                scale: 1.5f, effects: SpriteEffects.None, layerDepth: 0);
        }

        public void DrawPlayStatus(PlayStatus status) {
            spriteBatch.DrawString(SpriteFont, status.ToString (), new Vector2(20, 20), Color.Black);
        }

        public void DrawHeuristic(Player p, GameState gs, int x, int y) {
            var heuristic = Heuristic.heuristic (p, gs);
            var str = String.Format ("P{0}: {1:F1}", p.Id, heuristic);
            spriteBatch.DrawString(SpriteFont, str, new Vector2(x, y), Color.DarkViolet);
        }

        public void DrawGameObjectCircle(GameObject go, Color c) {
            Point pt = RasterizeCoords (go);
            Point dim = RasterizeDims (go);

            spriteBatch.Draw (FilledCircle(dim.X), (pt - new Point(dim.X, 0)).ToVector2(), c);
        }

//        http://stackoverflow.com/questions/2983809/how-to-draw-circle-with-specific-color-in-xna
//        public Texture2D CreateCircle(int radius)

        Dictionary<int, Texture2D> circleTexturesCache = new Dictionary<int, Texture2D>();
        public Texture2D FilledCircle(int radius) {
            Texture2D texture;

            if (circleTexturesCache.ContainsKey (radius)) {
                texture = circleTexturesCache [radius];
            } else {
                texture = CreateFilledCircle (radius);
                circleTexturesCache [radius] = texture;
            }

            return texture;
        }

        private Texture2D CreateFilledCircle(int radius) {
            int outerRadius = radius*2 + 2; // So circle doesn't go out of bounds
            Texture2D texture = new Texture2D(GraphicsDevice, outerRadius, outerRadius);

            Color[] data = new Color[outerRadius * outerRadius];

            int center = (int)Math.Round(outerRadius / 2f);

            for (int i = 0; i < data.Length; i++) {
                int row = i / outerRadius;
                int col = i % outerRadius;

                float dist = Util.euclideanDistance (new Vector2 (center, center), new Vector2 (row, col));

                data[i] = (dist <= radius) ? Color.White : Color.Transparent;
            }

            texture.SetData(data);

            return texture;
        }
	}
}

