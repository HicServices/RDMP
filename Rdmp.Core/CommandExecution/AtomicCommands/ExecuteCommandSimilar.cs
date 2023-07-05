// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.QueryBuilding;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
/// Find similar objects to an example e.g. all CHI columns in all datasets.  Optionally finds those with important differences only 
/// e.g. data type is different
/// </summary>
public class ExecuteCommandSimilar : BasicCommandExecution
{
    private readonly IMapsDirectlyToDatabaseTable _to;
    private bool _butDifferent;

    /// <summary>
    /// Collection of all Types where finding differences between instances is supported by 
    /// <see cref="Include(IMapsDirectlyToDatabaseTable)"/>
    /// </summary>
    private readonly Type[] _diffSupportedTypes = new Type[]{ typeof(ColumnInfo) };
    private ReadOnlyCollection<IMapsDirectlyToDatabaseTable> _matched;
    /// <summary>
    /// The objects matched by the command (similar or different objects)
    /// </summary>
    public ReadOnlyCollection<IMapsDirectlyToDatabaseTable> Matched {
        get {
            if (_matched == null)
                FetchMatches();
                
            return _matched;
        }
        set => _matched = value;
    }

    /// <summary>
    /// Set to true to make command show similar objects in interactive 
    /// </summary>
    public bool GoTo { get; set; }

    public ExecuteCommandSimilar(IBasicActivateItems activator,
        [DemandsInitialization("An object for which you want to find similar objects")]
        IMapsDirectlyToDatabaseTable to,
        [DemandsInitialization("True to show only objects that are similar (e.g. same name) but different (e.g. different data type)")]
        bool butDifferent):base(activator)
    {
        _to = to;
        _butDifferent = butDifferent;

        if(_butDifferent && !_diffSupportedTypes.Contains(_to.GetType()))
        {
            SetImpossible($"Differencing is not supported on {_to.GetType().Name}");
        }

        Weight = 50.3f;
    }

    public override void Execute()
    {
        FetchMatches();

        if (IsImpossible)
        {
            BasicActivator.Show("No Matches", ReasonCommandImpossible);
            return;
        }

        if (!BasicActivator.IsInteractive && GoTo)
        {
            throw new Exception($"GoTo property is true on {nameof(ExecuteCommandSimilar)} but activator is not interactive");
        }

        if(GoTo)
        {
            var selected = BasicActivator.SelectOne("Similar Objects", Matched.ToArray(), null, true);
            if(selected != null)
            {
                Emphasise(selected);
            }
        }
        else
        {
            BasicActivator.Show(string.Join(Environment.NewLine, Matched.ToArray().Select(ExecuteCommandDescribe.Describe)));
        }
    }

    public void FetchMatches()
    {
        try
        {
            var others = BasicActivator.CoreChildProvider.GetAllObjects(_to.GetType(), true);
            Matched = others.Where(IsSimilar).Where(Include).ToList().AsReadOnly();

            if (Matched.Count == 0)
            {
                SetImpossible(_butDifferent
                    ? "There are no alternate column specifications of this column"
                    : "There are no Similar objects");
            }
        }
        catch (Exception ex)
        {
            SetImpossible($"Error finding Similar:{ex.Message}");
        }
    }

    public override string GetCommandHelp()
    {
        return _butDifferent
            ? "Find objects with the same name but different implementation (e.g. different column data type)"
            : "Find other objects with the same or similar name to this";
    }

    private bool IsSimilar(IMapsDirectlyToDatabaseTable other)
    {
        // objects are not similar to themselves!
        if (Equals(_to, other))
        {
            return false;
        }

        return _to switch
        {
            INamed named when other is INamed otherNamed => SimilarWord(named.Name, otherNamed.Name,
                StringComparison.CurrentCultureIgnoreCase),
            IColumn col when other is IColumn otherCol => SimilarWord(col.SelectSQL, otherCol.SelectSQL,
                StringComparison.CurrentCultureIgnoreCase) || string.Equals(col.Alias, otherCol.Alias,
                StringComparison.CurrentCultureIgnoreCase),
            _ => false
        };
    }

    private static readonly char[] trimChars = new char[] { ' ', '[', ']', '\'', '"', '`' };

    private bool SimilarWord(string name1, string name2, StringComparison comparisonType)
    {
        if (string.IsNullOrWhiteSpace(name1) || string.IsNullOrWhiteSpace(name2))
            return false;

        return string.Equals(
            name1.Substring(Math.Max(0,name1.LastIndexOf('.'))).Trim(trimChars),
            name2.Substring(Math.Max(0,name2.LastIndexOf('.'))).Trim(trimChars),
            comparisonType);
    }

    private bool Include(IMapsDirectlyToDatabaseTable arg)
    {
        // if we don't care that they are different then return true
        if (!_butDifferent)
        {
            return true;
        }

        // or they are different
        if(_to is ColumnInfo col && arg is ColumnInfo otherCol)
        {
            return 
                !string.Equals(col.Data_type, otherCol.Data_type) || !string.Equals(col.Collation, otherCol.Collation);
        }

        // WHEN ADDING NEW TYPES add the Type to _diffSupportedTypes

        return false;
    }

}