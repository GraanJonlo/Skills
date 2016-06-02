using System.Collections.Generic;
using System.Linq;

namespace Moserware.Skills.FactorGraphs
{
    /// <summary>
    /// Helper class for computing the factor graph's normalization constant.
    /// </summary>    
    public class FactorList<TValue>
    {        
        private readonly List<Factor<TValue>> _list = new List<Factor<TValue>>();

        public double LogNormalization
        {
            get
            {                
                _list.ForEach(f => f.ResetMarginals());

                double sumLogZ = 0.0;
                                
                foreach (Factor<TValue> f in _list)
                {
                    for (int j = 0; j < f.NumberOfMessages; j++)
                    {
                        sumLogZ += f.SendMessage(j);
                    }
                }
                                
                double sumLogS = _list.Aggregate(0.0, (acc, fac) => acc + fac.LogNormalization);

                return sumLogZ + sumLogS;
            }
        }

        public int Count => _list.Count;

        public Factor<TValue> AddFactor(Factor<TValue> factor)
        {
            _list.Add(factor);
            return factor;
        }
    }
}