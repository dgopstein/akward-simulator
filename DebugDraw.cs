using System;
using Microsoft.Xna.Framework;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework.Graphics;

namespace awkwardsimulator
{
	public class DebugDraw : FarseerPhysics.DebugViewBase
	{
		Drawing drawing;

		public DebugDraw (Drawing drawing, World world) : base(world)
		{
			this.drawing = drawing;
		}

		override public void DrawCircle (Vector2 center, float radius, float red, float blue, float green) {
		}

		override public void DrawPolygon (Vector2[] vertices, int count, float red, float blue, float green, bool closed = true) {
			DrawPolygon(vertices, count, new Color(new Vector3(red, blue, green)), 2);
		}

		override public void DrawSegment (Vector2 start, Vector2 end, float red, float blue, float green) {
		}

		override public void DrawSolidCircle (Vector2 center, float radius, Vector2 axis, float red, float blue, float green) {
		}
	
		override public void DrawSolidPolygon (Vector2[] vertices, int count, float red, float blue, float green) {
		}

		override public void DrawTransform (ref Transform transform) {
		}

		// https://web.archive.org/web/20130718163159/http://www.xnawiki.com/index.php/Drawing_2D_lines_without_using_primitives
		public void DrawLineSegment(Vector2 point1, Vector2 point2, Color color, int lineWidth)
		{

			float angle = (float)Math.Atan2(point2.Y - point1.Y,
				point2.X - point1.X);
			float length = Vector2.Distance(point1, point2);

			drawing.SpriteBatch.Draw(drawing.BlankTexture, point1, null, color,
				angle, Vector2.Zero, new Vector2(length, lineWidth),
				SpriteEffects.None, 0f);
		}

		// https://bayinx.wordpress.com/2011/11/07/how-to-draw-lines-circles-and-polygons-using-spritebatch-in-xna/
		public void DrawPolygon(Vector2[] vertex, int count, Color color, int lineWidth)
		{
			if (count > 0)
			{
				for (int i = 0; i < count - 1; i++)
				{
					DrawLineSegment(vertex[i], vertex[i + 1], color, lineWidth);
				}
				DrawLineSegment(vertex[count - 1], vertex[0], color, lineWidth);
			}
		}
	}
}

