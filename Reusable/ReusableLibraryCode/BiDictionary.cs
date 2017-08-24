using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReusableLibraryCode
{
    //adapted from http://stackoverflow.com/questions/255341/getting-key-of-value-of-a-generic-dictionary#255630
    public class BiDictionary<TFirst, TSecond>
    {
        IDictionary<TFirst, TSecond> firstToSecond = new Dictionary<TFirst, TSecond>();
        IDictionary<TSecond, TFirst> secondToFirst = new Dictionary<TSecond, TFirst>();
        
        public ICollection<TFirst> Firsts { get { return firstToSecond.Keys; } }
        public ICollection<TSecond> Seconds { get { return firstToSecond.Values; } }

        public void Add(TFirst first, TSecond second)
        {
            if (firstToSecond.ContainsKey(first) ||
                secondToFirst.ContainsKey(second))
            {
                throw new ArgumentException("Duplicate first or second");
            }
            firstToSecond.Add(first, second);
            secondToFirst.Add(second, first);
        }

        public bool TryGetByFirst(TFirst first, out TSecond second)
        {
            return firstToSecond.TryGetValue(first, out second);
        }

        public bool TryGetBySecond(TSecond second, out TFirst first)
        {
            return secondToFirst.TryGetValue(second, out first);
        }

        public TFirst GetBySecond(TSecond second)
        {
            return secondToFirst[second];
        }

        public TSecond GetByFirst(TFirst first)
        {
            return firstToSecond[first];
        }

        public void RemoveByFirst(TFirst first)
        {
            var secondToRemove = firstToSecond[first];
            firstToSecond.Remove(first);
            secondToFirst.Remove(secondToRemove);
        }
        public void RemoveBySecond(TSecond second)
        {
            var secondToRemove = secondToFirst[second];
            secondToFirst.Remove(second);
            firstToSecond.Remove(secondToRemove);
        }

        public void Clear()
        {
            firstToSecond.Clear();
            secondToFirst.Clear();
        }
    }
}
