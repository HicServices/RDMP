// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using YamlDotNet.Serialization;

namespace Rdmp.Core.Curation.Data
{
    /// <summary>
    /// Tracks changes made by the user to one or more objects.  Changes are tracked from
    /// the moment the class is constructed until finish is called
    /// </summary>
    public class CommitInProgress
    {
        /// <summary>
        /// Tracks the original serialized yaml of each object tracked so that we can diff
        /// in <see cref="Finish"/> and create a <see cref="Commit"/> if necessary.
        /// </summary>
        Dictionary<IMapsDirectlyToDatabaseTable, string> oldYamls = new();

        /// <summary>
        /// Tracks the order of the last operation performed on each object so that
        /// <see cref="Memento"/> objects can be created in the order they were changed during
        /// the <see cref="Commit"/>.
        /// </summary>
        ConcurrentDictionary<IMapsDirectlyToDatabaseTable, int> orderOfLastOperations = new();

        private IRDMPPlatformRepositoryServiceLocator _locator;
        private ISerializer _serializer;

        int order = 0;

        public CommitInProgress(IRDMPPlatformRepositoryServiceLocator locator, params IMapsDirectlyToDatabaseTable[] track)
        {
            _locator = locator;
            _serializer = YamlRepository.CreateSerializer(
                _locator.GetAllRepositories().SelectMany(r=>r.GetCompatibleTypes()).Distinct()
                );

            foreach(var t in track)
            {
                oldYamls.Add(t, _serializer.Serialize(t));
                orderOfLastOperations.AddOrUpdate(t, -1,(k,v)=>-1);
                t.PropertyChanged += PropertyChanged;
            }
        }

        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var m = (IMapsDirectlyToDatabaseTable)sender;

            orderOfLastOperations.AddOrUpdate(m,
                Interlocked.Increment(ref order),
                (k, v) => Interlocked.Increment(ref order));
        }

        /// <summary>
        /// Returns a new <see cref="Commit"/> or null if nothing has changed with
        /// the tracked objects since construction.
        /// </summary>
        /// <returns></returns>
        public Commit Finish()
        {
            var changes = new Dictionary<IMapsDirectlyToDatabaseTable, Tuple<string,string>>();

            foreach (var t in oldYamls)
            {
                var newYaml =  _serializer.Serialize(t.Key);
                
                //something changed
                if(newYaml != t.Value)
                {
                    changes.Add(t.Key, Tuple.Create(t.Value, newYaml));
                }
            }

            if(!changes.Any())
            {
                // no changes so no need for a Commit
                return null;
            }
            var cataRepo = _locator.CatalogueRepository;

            var c = new Commit(cataRepo, Guid.NewGuid(), "TODO");

            foreach(var m in changes.OrderBy(c => orderOfLastOperations[c.Key]))
            {
                // TODO: Add/Delete too please!
                new Memento(cataRepo, c, MementoType.Modify, m.Key, m.Value.Item1, m.Value.Item2);
            }

            return c;
        }
    }
}
