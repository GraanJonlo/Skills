using System;

namespace Moserware.Skills.Numerics
{
    // The whole purpose of this class is to make the code for the SkillCalculator(s)
    // look a little cleaner

    public abstract class Range<T> where T : Range<T>, new()
    {
        private static readonly T Instance = new T();

        protected Range(int min, int max)
        {
            if (min > max)
            {
                throw new ArgumentOutOfRangeException();
            }

            Min = min;
            Max = max;
        }

        public int Min { get; }
        public int Max { get; }
        protected abstract T Create(int min, int max);

        // REVIEW: It's probably bad form to have access statics via a derived class, but the syntax looks better :-)

        public static T Inclusive(int min, int max)
        {
            return Instance.Create(min, max);
        }

        public static T Exactly(int value)
        {
            return Instance.Create(value, value);
        }

        public static T AtLeast(int minimumValue)
        {
            return Instance.Create(minimumValue, int.MaxValue);
        }

        public bool IsInRange(int value)
        {
            return (Min <= value) && (value <= Max);
        }
    }
}