using System;
using Microsoft.Xna.Framework;

namespace awkwardsimulator
{
    public class Util {

        public const float FIXED_DELTA_TIME = 0.2f; //TODO needs to be removed

        public static float clamp(float num, float min, float max) {
            num = ((num > min) ? 1 : 0) * num + ((!(num > min)) ? 1 : 0) * min;
            return ((num < max) ? 1 : 0) * num + ((!(num < max)) ? 1 : 0) * max;
        }

        public static float lerp(float a, float b, float t)
        {
            return a + (b-a) * t;
        }

        public static float moveTowards(float number, float target, float step)
        {
            float result = number;

            if (target > number)
            {
                result = result + step;
                if (result > target)
                    result = target;
            }

            if (target < number)
            {
                result = result - step;
                if (result < target)
                    result = target;
            }

            return result;
        }

        public static float euclideanDistance(Vector2 p1, Vector2 p2)
        {
            return (float) System.Math.Sqrt(System.Math.Pow(p2.X - p1.X, 2) + System.Math.Pow(p2.Y - p1.Y, 2));
        }

        public static double CosineSimilarity(Vector2 a, Vector2 b) {
            return Vector2.Dot (a, b) / (a.Length () * b.Length ());
        }

        public static bool SameDirection(Vector2 a, Vector2 b) {
            return CosineSimilarity (a, b) > 0;
        }
    }
}

