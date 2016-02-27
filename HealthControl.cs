using System;

namespace awkwardsimulator
{
    public class HealthControl
    {
        private static float health = 0.5f;

        private static float minSafeDistance = 4.0f;
        private static float maxSafeDistance = 6.0f;
        private static float rampWidth = 1.5f;
        private static float stasisWidth = 0.0f;
        private static float minDamagePerSecond = 0.004f;
        private static float maxDamagePerSecond = 0.01f;
        private static float maxHomeostasis = 0.01f;
        private static float minHomeostasis = 0.025f;

        /*
         *   -1: too awkward
         *    0: perfect
         *    1: too lonely
         */
        public static float Health(GameState state) {
            float dist = Util.euclideanDistance(state.p1.Coords, state.p2.Coords);
            float outOfBounds = 0.0f;

            if (dist > maxSafeDistance || dist < minSafeDistance)
            {
                if ((dist > maxSafeDistance && health > 0.5f) || (dist < minSafeDistance && health < 0.5f))
                    health = Util.moveTowards(health, 0.5f, maxHomeostasis * Util.FIXED_DELTA_TIME);
                outOfBounds = (dist > maxSafeDistance ? maxSafeDistance - dist : minSafeDistance - dist);

                float d = Util.lerp(minDamagePerSecond, maxDamagePerSecond, System.Math.Abs(outOfBounds / rampWidth));

                health = Util.clamp(health + d * ((float) Math.Sign(outOfBounds)) * Util.FIXED_DELTA_TIME, 0.0f, 1.0f);
            }
            else
            {
                outOfBounds = 0.0f;
                float midPoint = (maxSafeDistance + minSafeDistance) / 2.0f;
                float inBounds = (dist > midPoint ? maxSafeDistance - dist : dist - minSafeDistance);
                if (health != 0.5f && inBounds > stasisWidth)
                {
                    float heal = Util.lerp(minHomeostasis, maxHomeostasis, inBounds /* / stasisWidth*/);
                    health = Util.moveTowards(health, 0.5f, heal * Util.FIXED_DELTA_TIME);
                }
            }

            return 2*health - 1; // scale health from [-1, 1]
        }
    }
}

