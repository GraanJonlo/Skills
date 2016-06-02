using System;
using System.Collections.Generic;

namespace Moserware.Skills.FactorGraphs
{
    public abstract class Schedule<T>
    {
        private readonly string _name;

        protected Schedule(string name)
        {
            _name = name;
        }

        public abstract double Visit(int depth, int maxDepth);

        public double Visit()
        {
            return Visit(-1, 0);
        }
                
        public override string ToString()
        {
            return _name;
        }
    }

    public class ScheduleStep<T> : Schedule<T>
    {
        private readonly Factor<T> _factor;
        private readonly int _index;

        public ScheduleStep(string name, Factor<T> factor, int index)
            : base(name)
        {
            _factor = factor;
            _index = index;
        }

        public override double Visit(int depth, int maxDepth)
        {
            double delta = _factor.UpdateMessage(_index);
            return delta;
        }
    }
        
    public class ScheduleSequence<TValue> : ScheduleSequence<TValue, Schedule<TValue>>
    {
        public ScheduleSequence(string name, IEnumerable<Schedule<TValue>> schedules)
            : base(name, schedules)
        {
        }
    }

    public class ScheduleSequence<TValue, TSchedule> : Schedule<TValue>
        where TSchedule : Schedule<TValue>
    {
        private readonly IEnumerable<TSchedule> _schedules;

        public ScheduleSequence(string name, IEnumerable<TSchedule> schedules)
            : base(name)
        {
            _schedules = schedules;
        }

        public override double Visit(int depth, int maxDepth)
        {
            double maxDelta = 0;

            foreach (TSchedule currentSchedule in _schedules)
            {
                maxDelta = Math.Max(currentSchedule.Visit(depth + 1, maxDepth), maxDelta);
            }
            
            return maxDelta;
        }
    }

    public class ScheduleLoop<T> : Schedule<T>
    {
        private readonly double _maxDelta;
        private readonly Schedule<T> _scheduleToLoop;

        public ScheduleLoop(string name, Schedule<T> scheduleToLoop, double maxDelta)
            : base(name)
        {
            _scheduleToLoop = scheduleToLoop;
            _maxDelta = maxDelta;
        }

        public override double Visit(int depth, int maxDepth)
        {
            double delta = _scheduleToLoop.Visit(depth + 1, maxDepth);
            while (delta > _maxDelta)
            {
                delta = _scheduleToLoop.Visit(depth + 1, maxDepth);
            }

            return delta;
        }
    }
}