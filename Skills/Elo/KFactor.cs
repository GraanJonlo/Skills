
namespace Moserware.Skills.Elo
{
    public class KFactor
    {
        private readonly double _value;

        protected KFactor()
        {
        }

        public KFactor(double exactKFactor)
        {
            _value = exactKFactor;
        }

        public virtual double GetValueForRating(double rating)
        {
            return _value;
        }
    }
}
