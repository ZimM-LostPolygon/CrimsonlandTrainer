using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrimsonlandTrainer.Utility
{
    public static class IListExtensions
    {
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> range) {
            foreach (T x in range) {
                list.Add(x);
            }
        }
    }
}
