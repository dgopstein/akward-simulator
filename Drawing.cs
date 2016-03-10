using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using FarseerPhysics.Samples.DrawingSystem;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using FarseerPhysics.Dynamics;
using FarseerPhysics.DebugView;
using System.Linq;
using System.Diagnostics;

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

        private ContentManager content;
        public ContentManager Content { get { return content; } }

        private World world;
        public World World { get { return world; } }

        private Vector2 gameDimensions;

        public Drawing(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont spriteFont,
            ContentManager content, World world, Vector2 gameDimensions) {
			this.graphicsDevice = graphicsDevice;
			this.spriteBatch = spriteBatch;
			this.spriteFont = spriteFont;
            this.content = content;
            this.world = world;
            this.gameDimensions = gameDimensions;

			blankTexture = new Texture2D(graphicsDevice, 1, 1);
            blankTexture.SetData(new[] { Color.White });
		}

        Matrix debugProjMat;
        Matrix debugViewMat;
        DebugViewXNA DebugView;
        private void InitDebugDraw() {
            debugProjMat = Matrix.CreateOrthographicOffCenter(0f, gameDimensions.X, 0f, gameDimensions.Y, 0f, 1f);
            debugViewMat = Matrix.Identity;

            DebugView = new DebugViewXNA (world);
            DebugView.DefaultShapeColor = Color.White;
            DebugView.SleepingShapeColor = Color.LightGray;
            DebugView.LoadContent (GraphicsDevice, Content);
        }

        public void DrawDebug() {
            if (DebugView == null) { InitDebugDraw (); }

            DebugView.RenderDebugData(ref debugProjMat, ref debugViewMat); //XXX probably expensive
        }

        public float xScale() { return graphicsDevice.Viewport.Width  / gameDimensions.X; }
        public float yScale() { return graphicsDevice.Viewport.Height  / gameDimensions.Y; }

        public int xScale(float x) { return (int)Math.Round (xScale () * x); }
        public int yScale(float y) { return (int)Math.Round (yScale () * y); }

        public Point RasterizeCoords(GameObject go) {
            return RasterizeCoords (go.Coords) + new Point (0, (int)Math.Round (-yScale () * go.H));
		}

        public Point RasterizeCoords(Vector2 v) {
            return new Point (
                (int)Math.Round( xScale() * v.X),
                (int)Math.Round(-yScale() * v.Y + graphicsDevice.Viewport.Height)
            );
        }

        public Point RasterizeDims(GameObject go) {
            return RasterizeDims(go.Size);
        }

        public Point RasterizeDims(Vector2 v) {
            return new Point (xScale (v.X), yScale (v.Y));
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

        public void DrawHeuristic(AI ai, GameState state, int x, int y) {
            var heuristic = ai.Heuristic.Score(state);
            var str = String.Format ("P{0}: {1:F1}", ai.thisPlayer(state).Id, heuristic);
            spriteBatch.DrawString(SpriteFont, str, new Vector2(x, y), Color.DarkViolet);
        }

        public void DrawPos(Player p, int x, int y) {
            var str = String.Format ("P{0}: {1:F1}", p.Id, p.Coords);
            spriteBatch.DrawString(SpriteFont, str, new Vector2(x, y), Color.DarkViolet);
        }

        public void DrawCircle(float radius, Vector2 pos, Color c) {
            Point pt = RasterizeCoords (pos + (radius*new Vector2(-1, 1)));
            Point dim = RasterizeDims (new Vector2 (radius, 0));

            spriteBatch.Draw (FilledCircle(dim.X), pt.ToVector2(), c);
        }

        public void DrawGameObjectCircle(GameObjectCircle go, Color c) {
            DrawCircle(go.Radius, go.Center, c);
        }

        public void DrawTriangle(Vector2 size, Vector2 pos, float rotation, Color c) {
            Point pt = RasterizeCoords (pos);
            Point dim = RasterizeDims (size);

            var triangle = FilledTriangle (dim.X, dim.Y);

            spriteBatch.Draw (triangle, position: pt.ToVector2(), rotation: rotation,
                color: c, origin: .5f * dim.ToVector2());

        }

        public void DrawButtonArrow(Player p, Input input, Color c) {
            var size = new Vector2 (1f, 0.5f) * Player.Size;

            double rotation = 0;
            Vector2 offset;

//            Debug.WriteLine ("{0}: {1}", input.shortString(), input.ToInt);

            switch (input.ToInt) {
            case 1: //Input.Up.ToInt:
                rotation = 0;
                offset = new Vector2 (0, 1);
                break;
            case 2: //Input.Right.ToInt:
                rotation = 0.5;
                offset = new Vector2 (1, 0);
                break;
            case 3: //Input.UpRight.ToInt:
                rotation = 0.25;
                offset = new Vector2 (1, 1);
                break;
            case 4: //Input.Left.ToInt:
                rotation = -.5;
                offset = new Vector2 (-1, 0);
                break;
            case 5: //Input.UpLeft.ToInt:
                rotation = -.25;
                offset = new Vector2 (-1, 1);
                break;
            default: // This can happen when pressing left/right at the same time
                rotation = 1;
                offset = new Vector2 (0, -1);
//                throw new KeyNotFoundException ();
                break;
            }

            DrawTriangle (size, p.Center + (offset * Player.Size), (float)(Math.PI*rotation), c);
        }

//        http://stackoverflow.com/questions/2983809/how-to-draw-circle-with-specific-color-in-xna
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

        Dictionary<Tuple<int, int>, Texture2D> triangleTexturesCache = new Dictionary<Tuple<int, int>, Texture2D>();
        public Texture2D FilledTriangle(int width, int height) {
            Texture2D texture;

            var key = Tuple.Create (width, height);

            if (triangleTexturesCache.ContainsKey (key)) {
                texture = triangleTexturesCache [key];
            } else {
                texture = CreateFilledTriangle (width, height);
                triangleTexturesCache [key] = texture;
            }

            return texture;
        }
        private Texture2D CreateFilledTriangle(int width, int height) {
            Texture2D texture = new Texture2D(GraphicsDevice, width, height);

            Color[] data = new Color[width * height];

            for (int i = 0; i < data.Length; i++) {
                int row = i / height;
                int col = i % width;

                var center = width / 2;
                var dist = Math.Abs (center - col);

                data[i] = (dist <= row/2) ? Color.White : Color.Transparent;
            }

            texture.SetData(data);

            return texture;
        }
        Color[] colors = new Color[] {
            new Color(255, 255, 000),
            new Color(255, 127, 000),
            new Color(255, 000, 255),
            new Color(255, 000, 127),
            new Color(127, 255, 000),
            new Color(000, 255, 255),
            new Color(000, 255, 127),
            new Color(127, 000, 255),
            new Color(000, 127, 255),
            new Color(70, 70, 70)
            //new Color(255, 000, 000),
            //new Color(127, 000, 000),
            //new Color(000, 255, 000),
            //new Color(000, 127, 000),
            //new Color(000, 000, 255),
            //new Color(000, 000, 127),
            //new Color(000, 000, 000),
        };

        public void DrawPaths(IEnumerable<Tuple<double, IEnumerable<Vector2>>> paths) {
            if (paths.Count() == 0) return;

            int i = 0;

            var minH = paths.Min (x => x.Item1);
            var maxH = paths.Max (x => x.Item1);

            foreach (var tup in paths) {
                var h = (tup.Item1 - minH) / (maxH - minH);
                var path = tup.Item2;

                var c = HeatmapColor(1 - h);

                DrawPath (path, c, thickness: 2);
            }
        }

        public void DrawPath(IEnumerable<Vector2> paths, Color c, int thickness) {
            var segments = paths.Zip(paths.Skip(1), (a, b) => Tuple.Create(a, b));

            foreach (var tup in segments) {
                var v1 = RasterizeCoords (tup.Item1);
                var v2 = RasterizeCoords (tup.Item2);
                DrawLine(v1, v2, c, thickness);
            }
        }

        public void DrawLine(Vector2 start, Vector2 end, Color c = default(Color), int thickness = 1) {
            var startPt = RasterizeCoords (start);
            var endPt = RasterizeCoords (end);

            DrawLine (startPt, endPt, c, thickness);
        }

        // http://gamedev.stackexchange.com/questions/44015/how-can-i-draw-a-simple-2d-line-in-xna-without-using-3d-primitives-and-shders
        public void DrawLine(Point start, Point end, Color c = default(Color), int thickness = 1)
        {
            Point edge = end - start;
            // calculate angle to rotate line
            float angle =
                (float)Math.Atan2(edge.Y , edge.X);


            spriteBatch.Draw(blankTexture,
                new Rectangle(// rectangle defines shape of line and position of start of line
                    (int)start.X,
                    (int)start.Y,
                    (int)edge.ToVector2().Length(), //sb will strech the texture to fill this rectangle
                    thickness), //width of line, change this to make thicker line
                null,
                color: c, //colour of line
                rotation: angle,     //angle of line (calulated above)
                origin: new Vector2(0, 0), // point in line about which to rotate
                effects: SpriteEffects.None,
                layerDepth: 0);

        }

        public void DisposeTextures() {
            blankTexture.Dispose ();
            foreach (var tex in circleTexturesCache.Values) {
                tex.Dispose ();
            }
            foreach (var tex in triangleTexturesCache.Values) {
                tex.Dispose();
            }
        }

        private static List<Color> HeatmapColors = new List<Color>() {
//            Color.Blue, Color.Cyan, Color.Green, Color.Yellow, Color.Red};
            Color.Cyan, Color.Green, Color.Yellow, Color.Red};

        private static Color InterpolateColors(List<Color> colors, double value) {
            int nColors = colors.Count ();
            int index = (int)Math.Truncate (value * (nColors - 1));
            var distance = 1.0 / nColors;
            var d = (value % distance) / distance;

            if (index == nColors) {
                index = nColors - 1;
                d = 1;
            }

            Debug.WriteLineIf (index < 0, String.Format("index: {0}, value: {1}", index, value));

            var low = colors [index];
            var high = colors [index + (index == nColors - 1 ? 0 : 1)];

//            Debug.WriteLine ("{2} {0} {1}", index, d, value);
//            Debug.WriteLine ("{0} {1}", low, high);

            var c = new Color (
                (byte) ((1 - d) * low.R + d * high.R),
                (byte) ((1 - d) * low.G + d * high.G),
                (byte) ((1 - d) * low.B + d * high.B));

            return c;
        }

        private static Color HeatmapColor(double v) {
            return InterpolateColors(HeatmapColors, v);
        }
	}
}

