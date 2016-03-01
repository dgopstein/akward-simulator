using System;
using Microsoft.Xna.Framework;

namespace awkwardsimulator
{
    public class HealthControl
    {
        private static float health = 0.5f;

        private static float minSafeDistance = 10.0f;
        private static float maxSafeDistance = 2.0f * minSafeDistance;
        private static float rampWidth = 1.5f;
        private static float minDamagePerSecond = 0.001f;
        private static float maxDamagePerSecond = 0.004f;
        private static float healRate = 0.01f;
        private static float drainRate = 0.005f;

        /*
         *   -1: too awkward
         *    0: perfect
         *    1: too lonely
         */
        public static float Health(Vector2 c1, Vector2 c2) {
            float dist = Util.euclideanDistance(c1, c2);
            float outOfBounds = 0.0f;

            float IDEAL = 0.5f;

            if (dist > maxSafeDistance || dist < minSafeDistance)
            {
                if ((dist > maxSafeDistance && health > IDEAL) || (dist < minSafeDistance && health < IDEAL))
                    health = Util.moveTowards(health, IDEAL, drainRate * Util.FIXED_DELTA_TIME);
                outOfBounds = (dist > maxSafeDistance ? maxSafeDistance - dist : minSafeDistance - dist);

                float d = Util.lerp(minDamagePerSecond, maxDamagePerSecond, System.Math.Abs(outOfBounds / rampWidth));

                health = Util.clamp(health + d * ((float) Math.Sign(outOfBounds)) * Util.FIXED_DELTA_TIME, 0.0f, 1.0f);
            }
            else if (Math.Abs(health - IDEAL) > 0.0001) {
                float midPoint = (maxSafeDistance + minSafeDistance) / 2.0f;
                float windowSize = (maxSafeDistance - minSafeDistance);
                health = Util.moveTowards(health, IDEAL, healRate * ((windowSize/2) - Math.Abs(midPoint - dist)) * Util.FIXED_DELTA_TIME);
            }

            return 2*health - 1; // scale health from [-1, 1]
        }
    }
}

