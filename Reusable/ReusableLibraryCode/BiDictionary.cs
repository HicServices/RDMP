// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;

namespace ReusableLibraryCode
{
    //adapted from http://stackoverflow.com/questions/255341/getting-key-of-value-of-a-generic-dictionary#255630

    /// <summary>
    /// Bi directional Key Value pair class in which all Ks and Vs must be unique and which can be indexed either by K or V.
    /// </summary>
    /// <typeparam name="TFirst"></typeparam>
    /// <typeparam name="TSecond"></typeparam>
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
