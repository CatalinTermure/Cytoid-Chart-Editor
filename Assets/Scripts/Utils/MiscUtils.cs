using System;

namespace CCE.Utils
{
    public static class MiscUtils
    {
        public static T Clamp<T>(T val, T l, T r) where T : IComparable
        {
            if (val.CompareTo(l) == -1)
            {
                val = l;
            }
            else if (val.CompareTo(r) == 1) val = r;

            return val;
        }

        public static float GetDistance(float x1, float y1, float x2, float y2)
        {
            return (float)Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        }

        public static bool Approximately(double val, double target)
        {
            return Math.Abs(val - target) < 0.0001;
        }
    }
}