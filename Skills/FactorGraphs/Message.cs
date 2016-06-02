using System;

namespace Moserware.Skills.FactorGraphs
{
    public class Message<T>
    {
        private readonly string _nameFormat;
        private readonly object[] _nameFormatArgs;

        public Message()
            : this(default(T), null, null)
        {
        }

        public Message(T value, string nameFormat, params object[] args)

        {
            _nameFormat = nameFormat;
            _nameFormatArgs = args;
            Value = value;
        }

        public T Value { get; set; }

        public override string ToString()
        {
            return (_nameFormat == null) ? base.ToString() : String.Format(_nameFormat, _nameFormatArgs);
        }
    }
}