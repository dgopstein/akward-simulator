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

        public void DrawPlatform(Platform plat, Color c) {
            Point pt = RasterizeCoords (plat.Center + new Vector2(-1, 1));

            DrawGameObjectRect (plat, c);
            spriteBatch.DrawString(SpriteFont, plat.ToString(), pt.ToVector2(), Color.DarkViolet);
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

        public void DrawScore(PlayerId pId, float score, int x, int y) {
//            var heuristic = ai.Heuristic.Score(state);
            var str = String.Format ("score P{0}: {1:F1}", pId, score);
            spriteBatch.DrawString(SpriteFont, str, new Vector2(x, y), Color.DarkViolet);
        }

        public void DrawPos(Player p, int x, int y) {
            var str = String.Format ("coord P{0}: {1:F1}", p.Id, p.Coords);
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


        public void DrawPaths(IEnumerable<Tuple<double, IEnumerable<Vector2>>> paths, int thickness = 2) {
            if (paths.Count() == 0) return;

            var minH = paths.Min (x => x.Item1);
            var maxH = paths.Max (x => x.Item1);

            foreach (var tup in paths) {
                var h = (tup.Item1 - minH) / (maxH - minH);
                var path = tup.Item2;

                var c = HeatmapColor(1 - h);

                DrawPath (path, c, thickness: thickness);
            }
        }

        public void DrawPath(IEnumerable<Vector2> paths, Color c, int thickness) {
            DrawPath(paths.Select(v => Tuple.Create(c, v)), thickness);
        }

        public void DrawPath(IEnumerable<Tuple<Color, Vector2>> paths, int thickness) {
            var segments = paths.Zip(paths.Skip(1), (a, b) => Tuple.Create(a, b));

            foreach (var tup in segments) {
                var v1 = RasterizeCoords (tup.Item1.Item2);
                var v2 = RasterizeCoords (tup.Item2.Item2);
                DrawLine(v1, v2, tup.Item1.Item1, thickness);
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
            Color.Blue, Color.Cyan, Color.Green, Color.Yellow, Color.Red};
//            Color.Cyan, Color.Green, Color.Yellow, Color.Red};

        private static Color InterpolateColors(List<Color> colors, double value) {
            int nIndices = colors.Count () - 1;
            int index = (int)Math.Truncate (value * nIndices);
            var distance = 1.0 / nIndices;
            var d = (value % distance) / distance;

//            Debug.WriteLine ("color {0} - {1}/{2}", value, index, nColors);

            var low = colors [index];
            var high = colors [index + (index == nIndices ? 0 : 1)];

            var c = new Color (
                (byte) ((1 - d) * low.R + d * high.R),
                (byte) ((1 - d) * low.G + d * high.G),
                (byte) ((1 - d) * low.B + d * high.B));

            return c;
        }

        public static Color HeatmapColor(double v) {
            return InterpolateColors(HeatmapColors, v);
        }

        void DrawPlatformTargets(GameObject plat1, GameObject plat2, Color c) {

        }

        public void DrawMethodVisualizations(GameState state, CombinedAiInput cim) {
            Vector2 p1offset = new Vector2 (0, 5);


            DrawScore (PlayerId.P1, cim.Heuristic.Score(state), 20, 50);

            var combinedPath = cim.Heuristic.Path (state);

            // Draw platform targets
            for (int i = 0; i < combinedPath.Count; i++) {
                var val = i / (float)combinedPath.Count;
                Color c = Drawing.HeatmapColor (val);
                DrawCircle(2, combinedPath [i].Item1.Target + p1offset, c);
                DrawCircle(2, combinedPath [i].Item2.Target, c);
            }


            // Draw platform paths
            var platformPaths = cim.Heuristic.cpas.CombinedPlatformPath (
                                    state.P1, state.P2, state.Goal, state.Goal);
            var nPlats = (float)platformPaths.Count;
            DrawPath (platformPaths.Select ((tup, i) =>
                Tuple.Create(HeatmapColor(i / nPlats), tup.Item1.Target + new Vector2(0, p1offset.Y))), 2);
            DrawPath (platformPaths.Select ((tup, i) =>
                Tuple.Create(HeatmapColor(i / nPlats), tup.Item2.Target)), 2);

            // Draw target platforms
            var nextPlats = cim.Heuristic.cpas.NextPlatform(state.P1, state.P2, state.Goal, state.Goal);
            DrawCircle(1, nextPlats.Item1.Target + p1offset, Color.BlanchedAlmond);
            DrawCircle(1, nextPlats.Item2.Target, Color.BlanchedAlmond);

            // Draw heatmapped hypothetical player paths
            DrawPaths (cim.Ai.AllPaths().AsEnumerable().Reverse().Take(2).Select(t =>
                Tuple.Create(t.Item1, t.Item2.Select(e => e.Item2.P1.SurfaceCenter))));

            int i2 = 0;
            foreach (var path in cim.Ai.AllPaths().Take(1)) {
                var score = path.Item1;
                var coords1 = path.Item2.Last ().Item2.P1.SurfaceCenter;
                var coords2 = path.Item2.Last ().Item2.P1.SurfaceCenter;

                Debug.Print ("nextPath1: {0}", CombinedAi.moveListStr(path.Item2.Select(x => x.Item1.Item1)));
                Debug.Print ("nextPath2: {0}", CombinedAi.moveListStr(path.Item2.Select(x => x.Item1.Item2)));
            }
            DrawPaths (cim.Ai.AllPaths().AsEnumerable().Reverse().Take(2).Select(t =>
                Tuple.Create(t.Item1, t.Item2.Select(e => e.Item2.P2.SurfaceCenter))));
        }

        public void DrawMethodVisualizations(GameState state, AiInput aim) {
            DrawScore (aim.ai1.pId, aim.ai1.Heuristic.Score(state), 20, 50);
            DrawScore (aim.ai2.pId, aim.ai2.Heuristic.Score(state), 20, 80);

            if (aim.ai2.Heuristic is WaypointHeuristic) {
                WaypointHeuristic heuristic = (WaypointHeuristic)aim.ai2.Heuristic;
                DrawPath (heuristic.pas.PlatformPath (state.P2, state.Goal).Select (s => s.Target), Color.Maroon, 2);
                DrawCircle (2, heuristic.NextPlatform (state).Target, Color.Crimson);
            }

            // Draw heatmapped hypothetical player paths
            DrawPaths (aim.ai2.AllPaths().Select(t =>
                Tuple.Create(t.Item1, t.Item2.Select(e => e.Item2.P2.SurfaceCenter))));
            DrawPath (aim.ai1.AllPaths().First().Item2.Select(e => e.Item2.P1.SurfaceCenter),
                Color.WhiteSmoke, 3);
        }

	}
}

