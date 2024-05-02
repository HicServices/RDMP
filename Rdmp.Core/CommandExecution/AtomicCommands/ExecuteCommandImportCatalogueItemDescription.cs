// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Allows you to copy descriptive metadata (CatalogueItems) between datasets.  This is useful for maintaining a
///     'single version of the truth' e.g. if every dataset has a field called
///     'NHS Number' then the description of this column should be the same in every case.  Using this form you can
///     import/copy the description from another column.  While this covers you
///     for setting up new fields, the synchronizing of this description over time (e.g. when a data analyst edits one of
///     the other 'NHS Number' fields) is done through propagation
///     (See PropagateCatalogueItemChangesToSimilarNamedUI)
/// </summary>
public class ExecuteCommandImportCatalogueItemDescription : BasicCommandExecution, IAtomicCommand
{
    private readonly CatalogueItem _toPopulate;

    public ExecuteCommandImportCatalogueItemDescription(IBasicActivateItems activator, CatalogueItem toPopulate) :
        base(activator)
    {
        _toPopulate = toPopulate;
    }

    public override void Execute()
    {
        var available = BasicActivator.CoreChildProvider.AllCatalogueItems.Except(new[] { _toPopulate }).ToList();

        string initialSearchText = null;

        //if we have a CatalogueItem other than us that has same Name maybe that's the one they want
        if (available.Any(a => a.Name.Equals(_toPopulate.Name, StringComparison.CurrentCultureIgnoreCase)))
            initialSearchText = _toPopulate.Name;

        if (SelectOne(available, out var selected, initialSearchText))
        {
            CopyNonIDValuesAcross(selected, _toPopulate, true);
            _toPopulate.SaveToDatabase();

            Publish(_toPopulate);
        }

        base.Execute();
    }

    /// <summary>
    ///     Copies all properties (Description etc) from one CatalogueItem into another (except ID properties).
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="skipNameProperty"></param>
    public static void CopyNonIDValuesAcross(IMapsDirectlyToDatabaseTable from, IMapsDirectlyToDatabaseTable to,
        bool skipNameProperty = false)
    {
        var type = from.GetType();

        if (to.GetType() != type)
            throw new Exception("From and To objects must be of the same Type");

        foreach (var propertyInfo in type.GetProperties())
        {
            if (propertyInfo.Name == "ID")
                continue;

            if (propertyInfo.Name.EndsWith("_ID"))
                continue;

            if (propertyInfo.Name == "Name" && skipNameProperty)
                continue;

            if (propertyInfo.CanWrite == false || propertyInfo.CanRead == false)
                continue;

            var value = propertyInfo.GetValue(from, null);
            propertyInfo.SetValue(to, value, null);
        }

        if (to is ISaveable s)
            s.SaveToDatabase();
    }
}