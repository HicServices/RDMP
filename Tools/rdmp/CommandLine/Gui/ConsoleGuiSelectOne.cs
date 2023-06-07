// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui;

class ConsoleGuiSelectOne : ConsoleGuiBigListBox<IMapsDirectlyToDatabaseTable>
{
    private readonly IBasicActivateItems _activator;
    private readonly Dictionary<IMapsDirectlyToDatabaseTable, DescendancyList> _masterCollection;
    private SearchablesMatchScorer _scorer;
    private TextField txtId;

    /// <summary>
    /// The maximum number of objects to show in the list box
    /// </summary>
    public const int MaxMatches = 100;
        
    public ConsoleGuiSelectOne(IBasicActivateItems activator, IEnumerable<IMapsDirectlyToDatabaseTable> available):base("Open","Ok",
        true,null)
    {
        _activator = activator;
            
        if(available != null)
        {
            _masterCollection = available.ToDictionary(k=>k,v=>activator.CoreChildProvider.GetDescendancyListIfAnyFor(v));
        }
        else
        {
            _masterCollection = _activator.CoreChildProvider.GetAllSearchables();
        }
            
        _publicCollection = _masterCollection.Select(v=>v.Key).ToList();
        SetAspectGet(_activator.CoreChildProvider);
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

    protected override void AddMoreButtonsAfter(Window win, Button btnCancel)
    {
        var lbl = new Label("ID:"){
            X = Pos.Right(btnCancel) + 1,
            Y = Pos.Top(btnCancel)
        };
        win.Add(lbl);
            
        txtId = new TextField
        {
            X = Pos.Right(lbl),
            Y = Pos.Top(lbl),
            Width = 5
        };

        txtId.TextChanged += s=>RestartFiltering();

        win.Add(txtId);
    }


    protected override IList<IMapsDirectlyToDatabaseTable> GetListAfterSearch(string searchText, CancellationToken token)
    {
        if(token.IsCancellationRequested)
            return new List<IMapsDirectlyToDatabaseTable>();
             
        if(int.TryParse(txtId.Text.ToString(), out var searchForID))
            _scorer.ID = searchForID;
        else 
            _scorer.ID = null;

        var dict = _scorer.ScoreMatches(_masterCollection, searchText, token,null);

        //can occur if user punches many keys at once
        if(dict == null)
            return new List<IMapsDirectlyToDatabaseTable>();

        return _scorer.ShortList(dict, MaxMatches,_activator);
    }
}