// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace Rdmp.Core.Curation.Data
{
    /// <summary>
    /// Tracks changes made by the user to one or more objects.  Changes are tracked from
    /// the moment the class is constructed until finish is called
    /// </summary>
    public class CommitInProgress
    {
        Dictionary<IMapsDirectlyToDatabaseTable, string> oldYamls = new();
        private IRDMPPlatformRepositoryServiceLocator _locator;
        private ISerializer _serializer;

        public CommitInProgress(IRDMPPlatformRepositoryServiceLocator locator, params IMapsDirectlyToDatabaseTable[] track)
        {
            _locator = locator;
            _serializer = YamlRepository.CreateSerializer(
                _locator.GetAllRepositories().SelectMany(r=>r.GetCompatibleTypes()).Distinct()
                );

            foreach(var t in track)
            {
                oldYamls.Add(t, _serializer.Serialize(t));
            }
        }

        // TODO: order changes in the sequence they happened
        // TODO: track new/deleted things

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
            foreach(var m in changes)
            {
                // TODO: Add/Delete too please!
                new Memento(cataRepo, c, MementoType.Modify, m.Key, m.Value.Item1, m.Value.Item2);
            }

            return c;
        }
    }
}
