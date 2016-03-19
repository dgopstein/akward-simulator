using System;
using Microsoft.Xna.Framework;

namespace awkwardsimulator
{
    public class HealthControl
    {
        public const float minSafeDistance = 10.0f;
        public const float maxSafeDistance = 2.0f * minSafeDistance;

        public static float IdealDistance { get { return (minSafeDistance + maxSafeDistance) / 2f; } }
        public static float SafeWindowSize{ get { return maxSafeDistance - minSafeDistance; } }

        private const float rampWidth = 1.5f;
        private const float minDamagePerSecond = 0.001f;
        private const float maxDamagePerSecond = 0.004f;
        private const float healRate = 0.01f;
        private const float drainRate = 0.005f;

        /*
         *   -1: too awkward
         *    0: perfect
         *    1: too lonely
         */
        public static float Health(float oldHealth1, Vector2 c1, Vector2 c2) {
            float dist = Util.euclideanDistance(c1, c2);
            float outOfBounds = 0.0f;

            float oldHealth = (oldHealth1 + 1f) / 2f;
            float LOW = 0f, IDEAL = 0.5f, HIGH = 1f;

            float newHealth = oldHealth;

            if (dist > maxSafeDistance || dist < minSafeDistance)
            {
                if ((dist > maxSafeDistance && oldHealth > IDEAL) || (dist < minSafeDistance && oldHealth < IDEAL))
                    newHealth = Util.moveTowards(oldHealth, IDEAL, drainRate * Util.FIXED_DELTA_TIME);
                outOfBounds = (dist > maxSafeDistance ? maxSafeDistance - dist : minSafeDistance - dist);

                float d = Util.lerp(minDamagePerSecond, maxDamagePerSecond, System.Math.Abs(outOfBounds / rampWidth));

                newHealth = Util.clamp(oldHealth + d * ((float) Math.Sign(outOfBounds)) * Util.FIXED_DELTA_TIME, LOW, HIGH);
            }
            else if (Math.Abs(oldHealth - IDEAL) > 0.0001) {
                newHealth = Util.moveTowards(oldHealth, IDEAL, healRate * ((SafeWindowSize/2) - Math.Abs(IdealDistance - dist)) * Util.FIXED_DELTA_TIME);
            }

            return 2*newHealth - 1; // scale health from [-1, 1]
        }
    }
}

