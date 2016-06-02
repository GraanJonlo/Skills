using System;
using System.Collections.Generic;

namespace Moserware.Skills.FactorGraphs
{
    public abstract class FactorGraphLayerBase<TValue>
    {
        public abstract IEnumerable<Factor<TValue>> UntypedFactors { get; }
        public abstract void BuildLayer();

        public virtual Schedule<TValue> CreatePriorSchedule()
        {
            return null;
        }

        public virtual Schedule<TValue> CreatePosteriorSchedule()
        {
            return null;
        }

        // HACK

        public abstract void SetRawInputVariablesGroups(object value);
        public abstract object GetRawOutputVariablesGroups();
    }

    public abstract class FactorGraphLayer<TParentGraph, TValue, TBaseVariable, TInputVariable, TFactor, TOutputVariable>
        : FactorGraphLayerBase<TValue>
        where TParentGraph : FactorGraph<TParentGraph, TValue, TBaseVariable>
        where TBaseVariable : Variable<TValue>
        where TInputVariable : TBaseVariable
        where TFactor : Factor<TValue>
        where TOutputVariable : TBaseVariable
    {
        private readonly List<TFactor> _localFactors = new List<TFactor>();
        private readonly List<IList<TOutputVariable>> _outputVariablesGroups = new List<IList<TOutputVariable>>();
        private IList<IList<TInputVariable>> _inputVariablesGroups = new List<IList<TInputVariable>>();

        protected FactorGraphLayer(TParentGraph parentGraph)
        {
            ParentFactorGraph = parentGraph;
        }

        protected IList<IList<TInputVariable>> InputVariablesGroups => _inputVariablesGroups;

        // HACK

        public TParentGraph ParentFactorGraph { get; private set; }

        public IList<IList<TOutputVariable>> OutputVariablesGroups => _outputVariablesGroups;

        public IList<TFactor> LocalFactors => _localFactors;

        public override IEnumerable<Factor<TValue>> UntypedFactors => _localFactors;

        public override void SetRawInputVariablesGroups(object value)
        {
            var newList = value as IList<IList<TInputVariable>>;
            if (newList == null)
            {
                // TODO: message
                throw new ArgumentException();
            }

            _inputVariablesGroups = newList;
        }

        public override object GetRawOutputVariablesGroups()
        {
            return _outputVariablesGroups;
        }

        protected Schedule<TValue> ScheduleSequence<TSchedule>(
            IEnumerable<TSchedule> itemsToSequence,
            string nameFormat,
            params object[] args)
            where TSchedule : Schedule<TValue>

        {
            string formattedName = String.Format(nameFormat, args);
            return new ScheduleSequence<TValue, TSchedule>(formattedName, itemsToSequence);
        }

        protected void AddLayerFactor(TFactor factor)
        {
            _localFactors.Add(factor);
        }

        // Helper utility
        protected double Square(double x)
        {
            return x*x;
        }
    }
}