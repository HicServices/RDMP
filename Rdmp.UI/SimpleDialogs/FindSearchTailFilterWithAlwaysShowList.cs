﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using BrightIdeasSoftware;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Providers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.UI.SimpleDialogs;

internal class FindSearchTailFilterWithAlwaysShowList : IListFilter
{
    private List<IMapsDirectlyToDatabaseTable> _scoringObjects;

    public IEnumerable<object> AlwaysShow { get; }
    public CancellationToken CancellationToken { get; }

    public FindSearchTailFilterWithAlwaysShowList(IBasicActivateItems activator, IEnumerable<object> alwaysShow,
        IEnumerable<IMapsDirectlyToDatabaseTable> allObjects, string text, int maxToTake,
        CancellationToken cancellationToken)
    {
        AlwaysShow = alwaysShow;
        CancellationToken = cancellationToken;

        if (string.IsNullOrEmpty(text))
        {
            _scoringObjects = allObjects.Take(maxToTake).ToList();
        }
        else
        {
            var searchThese = allObjects.ToDictionary(o => o, activator.CoreChildProvider.GetDescendancyListIfAnyFor);

            var scorer = new SearchablesMatchScorer
            {
                TypeNames = new HashSet<string>(allObjects.Select(m => m.GetType().Name).Distinct(), StringComparer.CurrentCultureIgnoreCase)
            };
            var matches = scorer.ScoreMatches(searchThese, text,null, cancellationToken);

            // we were cancelled
            if (matches == null)
            {
                _scoringObjects = new List<IMapsDirectlyToDatabaseTable>();
                return;
            }

            _scoringObjects = SearchablesMatchScorer.ShortList(matches, maxToTake, activator);
        }
    }


    public IEnumerable Filter(IEnumerable modelObjects) => _scoringObjects.Union(AlwaysShow);
}