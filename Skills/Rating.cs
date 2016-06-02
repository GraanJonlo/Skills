using System;
using Moserware.Numerics;

namespace Moserware.Skills
{
    /// <summary>
    /// Container for a player's rating.
    /// </summary>
    public class Rating
    {
        private const int ConservativeStandardDeviationMultiplier = 3;
        private readonly double _conservativeStandardDeviationMultiplier;
        private readonly double _mean;
        private readonly double _standardDeviation;

        /// <summary>
        /// Constructs a rating.
        /// </summary>
        /// <param name="mean">The statistical mean value of the rating (also known as μ).</param>
        /// <param name="standardDeviation">The standard deviation of the rating (also known as σ).</param>        
        public Rating(double mean, double standardDeviation)
            : this(mean, standardDeviation, ConservativeStandardDeviationMultiplier)
        {
        }

        /// <summary>
        /// Constructs a rating.
        /// </summary>
        /// <param name="mean">The statistical mean value of the rating (also known as μ).</param>
        /// <param name="standardDeviation">The standard deviation (the spread) of the rating (also known as σ).</param>
        /// <param name="conservativeStandardDeviationMultiplier">The number of <paramref name="standardDeviation"/>s to subtract from the <paramref name="mean"/> to achieve a conservative rating.</param>
        public Rating(double mean, double standardDeviation, double conservativeStandardDeviationMultiplier)
        {
            _mean = mean;
            _standardDeviation = standardDeviation;
            _conservativeStandardDeviationMultiplier = conservativeStandardDeviationMultiplier;
        }

        /// <summary>
        /// The statistical mean value of the rating (also known as μ).
        /// </summary>
        public double Mean => _mean;

        /// <summary>
        /// The standard deviation (the spread) of the rating. This is also known as σ.
        /// </summary>
        public double StandardDeviation => _standardDeviation;

        /// <summary>
        /// A conservative estimate of skill based on the mean and standard deviation.
        /// </summary>
        public double ConservativeRating => _mean - _conservativeStandardDeviationMultiplier*_standardDeviation;

        public static Rating GetPartialUpdate(Rating prior, Rating fullPosterior, double updatePercentage)
        {
            var priorGaussian = new GaussianDistribution(prior.Mean, prior.StandardDeviation);
            var posteriorGaussian = new GaussianDistribution(fullPosterior.Mean, fullPosterior.StandardDeviation);

            // From a clarification email from Ralf Herbrich:
            // "the idea is to compute a linear interpolation between the prior and posterior skills of each player 
            //  ... in the canonical space of parameters"

            double precisionDifference = posteriorGaussian.Precision - priorGaussian.Precision;
            double partialPrecisionDifference = updatePercentage*precisionDifference;

            double precisionMeanDifference = posteriorGaussian.PrecisionMean - priorGaussian.PrecisionMean;
            double partialPrecisionMeanDifference = updatePercentage*precisionMeanDifference;

            GaussianDistribution partialPosteriorGaussion = GaussianDistribution.FromPrecisionMean(
                priorGaussian.PrecisionMean + partialPrecisionMeanDifference,
                priorGaussian.Precision + partialPrecisionDifference);

            return new Rating(partialPosteriorGaussion.Mean, partialPosteriorGaussion.StandardDeviation,
                              prior._conservativeStandardDeviationMultiplier);
        }

        public override string ToString()
        {
            // As a debug helper, display a localized rating:
            return $"μ={Mean:0.0000}, σ={StandardDeviation:0.0000}";
        }
    }
}