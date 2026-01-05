// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.UI;
using Rdmp.UI.Collections;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;

/// <summary>
/// A Document Tab that hosts an RDMPCollection, the control knows how to save itself to the persistence settings file for the user ensuring that when they next open the
/// software the Tab can be reloaded and displayed.  Persistence involves storing this Tab type, the Collection Control type being hosted by the Tab (an RDMPCollection).
/// Since there can only ever be one RDMPCollection of any Type active at a time this is all that must be stored to persist the control
/// </summary>
[TechnicalUI]
[System.ComponentModel.DesignerCategory("")]
public class PersistableToolboxDockContent : DockContent
{
    public const string Prefix = "Toolbox";

    public readonly RDMPCollection CollectionType;

    public PersistableToolboxDockContent(RDMPCollection collectionType)
    {
        CollectionType = collectionType;
    }

    protected override string GetPersistString()
    {
        var args = new Dictionary<string, string>
        {
            { "Toolbox", CollectionType.ToString() }
        };

        return $"{Prefix}{PersistStringHelper.Separator}{PersistStringHelper.SaveDictionaryToString(args)}";
    }

    public RDMPCollectionUI GetCollection() => Controls.OfType<RDMPCollectionUI>().SingleOrDefault();


    public static RDMPCollection? GetToolboxFromPersistString(string persistString)
    {
        var s = persistString[(Prefix.Length + 1)..];

        var args = PersistStringHelper.LoadDictionaryFromString(s);

        return args.TryGetValue("Toolbox", out var toolbox) &&
               Enum.TryParse(toolbox, true, out RDMPCollection collection)
            ? collection
            : null;
    }
}