﻿using System;
using System.Collections.Generic;
using System.Linq;
using Moserware.Numerics;
using Moserware.Skills.FactorGraphs;
using Moserware.Skills.TrueSkill.Layers;

namespace Moserware.Skills.TrueSkill
{
    public class TrueSkillFactorGraph<TPlayer> :
        FactorGraph<TrueSkillFactorGraph<TPlayer>, GaussianDistribution, Variable<GaussianDistribution>>
    {
        private readonly List<FactorGraphLayerBase<GaussianDistribution>> _layers;
        private readonly PlayerPriorValuesToSkillsLayer<TPlayer> _priorLayer;

        public TrueSkillFactorGraph(GameInfo gameInfo, IEnumerable<IDictionary<TPlayer, Rating>> teams, int[] teamRanks)
        {
            _priorLayer = new PlayerPriorValuesToSkillsLayer<TPlayer>(this, teams);
            GameInfo = gameInfo;
            VariableFactory =
                new VariableFactory<GaussianDistribution>(() => GaussianDistribution.FromPrecisionMean(0, 0));

            _layers = new List<FactorGraphLayerBase<GaussianDistribution>>
                          {
                              _priorLayer,
                              new PlayerSkillsToPerformancesLayer<TPlayer>(this),
                              new PlayerPerformancesToTeamPerformancesLayer<TPlayer>(this),
                              new IteratedTeamDifferencesInnerLayer<TPlayer>(
                                  this,
                                  new TeamPerformancesToTeamPerformanceDifferencesLayer<TPlayer>(this),
                                  new TeamDifferencesComparisonLayer<TPlayer>(this, teamRanks))
                          };
        }

        public GameInfo GameInfo { get; private set; }

        public void BuildGraph()
        {
            object lastOutput = null;

            foreach (var currentLayer in _layers)
            {
                if (lastOutput != null)
                {
                    currentLayer.SetRawInputVariablesGroups(lastOutput);
                }

                currentLayer.BuildLayer();

                lastOutput = currentLayer.GetRawOutputVariablesGroups();
            }
        }

        public void RunSchedule()
        {
            Schedule<GaussianDistribution> fullSchedule = CreateFullSchedule();
            fullSchedule.Visit();
        }

        public double GetProbabilityOfRanking()
        {
            var factorList = new FactorList<GaussianDistribution>();

            foreach (var currentLayer in _layers)
            {
                foreach (var currentFactor in currentLayer.UntypedFactors)
                {
                    factorList.AddFactor(currentFactor);
                }
            }

            double logZ = factorList.LogNormalization;
            return Math.Exp(logZ);
        }

        private Schedule<GaussianDistribution> CreateFullSchedule()
        {
            var fullSchedule = new List<Schedule<GaussianDistribution>>();

            foreach (var currentLayer in _layers)
            {
                Schedule<GaussianDistribution> currentPriorSchedule = currentLayer.CreatePriorSchedule();
                if (currentPriorSchedule != null)
                {
                    fullSchedule.Add(currentPriorSchedule);
                }
            }

            // Casting to IEnumerable to get the LINQ Reverse()
            IEnumerable<FactorGraphLayerBase<GaussianDistribution>> allLayers = _layers;

            foreach (var currentLayer in allLayers.Reverse())
            {
                Schedule<GaussianDistribution> currentPosteriorSchedule = currentLayer.CreatePosteriorSchedule();
                if (currentPosteriorSchedule != null)
                {
                    fullSchedule.Add(currentPosteriorSchedule);
                }
            }

            return new ScheduleSequence<GaussianDistribution>("Full schedule", fullSchedule);
        }

        public IDictionary<TPlayer, Rating> GetUpdatedRatings()
        {
            var result = new Dictionary<TPlayer, Rating>();
            foreach (var currentTeam in _priorLayer.OutputVariablesGroups)
            {
                foreach (var currentPlayer in currentTeam)
                {
                    result[currentPlayer.Key] = new Rating(currentPlayer.Value.Mean,
                                                           currentPlayer.Value.StandardDeviation);
                }
            }

            return result;
        }
    }
}