// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers;

namespace Rdmp.Core.CommandLine.Gui
{
    class ConsoleGuiSelectOne : ConsoleGuiBigListBox<IMapsDirectlyToDatabaseTable>
    {
        private readonly Dictionary<IMapsDirectlyToDatabaseTable, DescendancyList> _masterCollection;
        private SearchablesMatchScorer _scorer;

        /// <summary>
        /// The maximum number of objects to show in the list box
        /// </summary>
        public const int MaxMatches = 100;

        private ConsoleGuiSelectOne():base("Open","Ok",true,null)
        {
            
        }
        
        public ConsoleGuiSelectOne(ICoreChildProvider childProvider):this()
        {
            _masterCollection = childProvider.GetAllSearchables();
            SetAspectGet(childProvider);
        }

        private void SetAspectGet(ICoreChildProvider childProvider)
        {
            AspectGetter = (o) =>
            {
                if (o == null)
                    return "Null";

                var parent = childProvider.GetDescendancyListIfAnyFor(o)?.GetMostDescriptiveParent();
                
                return parent != null ? $"{o.ID} {o.GetType().Name} {o} ({parent})" : $"{o.ID} {o.GetType().Name} {o}";
            };

            _scorer = new SearchablesMatchScorer();
            _scorer.TypeNames = new HashSet<string>(_masterCollection.Select(m => m.Key.GetType().Name).Distinct(),StringComparer.CurrentCultureIgnoreCase);

        }

        public ConsoleGuiSelectOne(ICoreChildProvider coreChildProvider, IMapsDirectlyToDatabaseTable[] availableObjects):this()
        {
            _masterCollection = coreChildProvider.GetAllSearchables().Where(k=> availableObjects.Contains(k.Key)).ToDictionary(k=>k.Key,v=>v.Value);
            SetAspectGet(coreChildProvider);
        }

        protected override IList<IMapsDirectlyToDatabaseTable> GetListAfterSearch(string searchText, CancellationToken token)
        {
            if(token.IsCancellationRequested)
                return new List<IMapsDirectlyToDatabaseTable>();
             
            var dict = _scorer.ScoreMatches(_masterCollection, searchText, token,null);

            //can occur if user punches many keys at once
            if(dict == null)
                return new List<IMapsDirectlyToDatabaseTable>();

            return
                dict
                .Where(score => score.Value > 0)
                .OrderByDescending(score => score.Value)
                .ThenByDescending(id => id.Key.Key.ID) //favour newer objects over ties
                .Take(MaxMatches)
                .Select(score => score.Key.Key)
                .ToList();
        }

        protected override IList<IMapsDirectlyToDatabaseTable> GetInitialSource()
        {
            return _masterCollection.Keys.ToList();
        }
    }
}