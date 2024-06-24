// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Settings;

namespace Rdmp.UI.Collections.Providers;

/// <summary>
/// Tracks user access of objects over time and stores in local persistence file <see cref="UserSettings.RecentHistory"/>
/// </summary>
public class HistoryProvider
{
    /// <summary>
    /// Collection of objects and when they were accessed, use <see cref="HistoryProvider.Add"/> instead of modifying this list directly
    /// </summary>
    public List<HistoryEntry> History { get; set; } = new();

    /// <summary>
    /// What Types to track in <see cref="Add"/>
    /// </summary>
    public Type[] TrackTypes { get; set; } =
    {
        typeof(Catalogue),
        typeof(Project),
        typeof(ExtractionConfiguration),
        typeof(CohortIdentificationConfiguration),
        typeof(LoadMetadata)
    };

    /// <summary>
    /// Creates a new history provider and loads the users history from the persistence file (<see cref="UserSettings.RecentHistory"/>)
    /// </summary>
    /// <param name="locator"></param>
    public HistoryProvider(IRDMPPlatformRepositoryServiceLocator locator)
    {
        try
        {
            var history = UserSettings.RecentHistory;

            if (string.IsNullOrWhiteSpace(history))
                return;

            foreach (var s in history.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                var entry = HistoryEntry.Deserialize(s, locator);

                if (entry != null)
                    History.Add(entry);
            }
        }
        catch (Exception)
        {
            //error reading persisted history, maybe history file is corrupt or something
        }
    }

    /// <summary>
    /// Saves into user settings the supplied number of <see cref="HistoryEntry"/> (per Type)
    /// </summary>
    /// <param name="numberOfEntries">Maximum number of objects of any given <see cref="System.Type"/> to store</param>
    public void Save(int numberOfEntries = 20)
    {
        var newHistory = new List<HistoryEntry>();
        var sb = new StringBuilder();

        foreach (var group in History.GroupBy(o => o.Object.GetType()))
        {
            var recentsOfType = group.ToList().OrderByDescending(e => e.Date).Take(numberOfEntries).ToList();

            //save x of each Type
            sb.AppendLine(string.Join(Environment.NewLine, recentsOfType.Select(h => h.Serialize())));
            newHistory.AddRange(recentsOfType);
        }

        History = newHistory;

        UserSettings.RecentHistory = sb.ToString();
    }

    /// <summary>
    /// Adds the <paramref name="o"/> to the history (assuming it is one of the <see cref="TrackTypes"/>).  New entry replaces any previous entries for <paramref name="o"/>.  This method causes implicit <see cref="Save"/>
    /// </summary>
    /// <param name="o"></param>
    public void Add(IMapsDirectlyToDatabaseTable o)
    {
        if (o == null)
            return;

        if (!TrackTypes.Contains(o.GetType()))
            return;

        var newEntry = new HistoryEntry(o, DateTime.Now);

        //chuck old record (and old date - HistoryEntry are the same if object is the same)
        if (History.Contains(newEntry))
            History.Remove(newEntry);

        //add new record
        History.Add(newEntry);
        Save();
    }

    /// <summary>
    /// Clears the users recent history of objects accessed including in the persistence file (<see cref="UserSettings.RecentHistory"/>)
    /// </summary>
    public void Clear()
    {
        UserSettings.RecentHistory = "";
        History.Clear();
    }

    public void Remove(IMapsDirectlyToDatabaseTable o)
    {
        if (o == null)
            return;

        foreach (var historyEntry in History.Where(h => h.Object.Equals(o)).ToArray())
            History.Remove(historyEntry);
    }
}