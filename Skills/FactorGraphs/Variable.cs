using System;

namespace Moserware.Skills.FactorGraphs
{
    public class Variable<TValue>
    {
        private readonly string _name;
        private readonly TValue _prior;

        public Variable(string name, TValue prior)
        {
            _name = "Variable[" + name + "]";
            _prior = prior;
            ResetToPrior();
        }

        public virtual TValue Value { get; set; }

        public void ResetToPrior()
        {
            Value = _prior;
        }

        public override string ToString()
        {
            return _name;
        }
    }

    public class DefaultVariable<TValue> : Variable<TValue>
    {
        public DefaultVariable()
            : base("Default", default(TValue))
        {
        }

        public override TValue Value
        {
            get { return default(TValue); }
            set { throw new NotSupportedException(); }
        }
    }

    public class KeyedVariable<TKey, TValue> : Variable<TValue>
    {
        public KeyedVariable(TKey key, string name, TValue prior)
            : base(name, prior)
        {
            Key = key;
        }

        public TKey Key { get; private set; }
    }
}