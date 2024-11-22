// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ExtractionUIs.JoinsAndLookups;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandAddNewLookupTableRelationship : BasicUICommandExecution
{
    private readonly Catalogue _catalogueIfKnown;
    private readonly TableInfo _lookupTableInfoIfKnown;

    public ExecuteCommandAddNewLookupTableRelationship(IActivateItems activator, Catalogue catalogueIfKnown,
        TableInfo lookupTableInfoIfKnown) : base(activator)
    {
        _catalogueIfKnown = catalogueIfKnown;
        _lookupTableInfoIfKnown = lookupTableInfoIfKnown;

        if (catalogueIfKnown != null && catalogueIfKnown.IsApiCall())
            SetImpossible("Lookups cannot be configured on API Catalogues");


        if (catalogueIfKnown == null && lookupTableInfoIfKnown == null)
            throw new NotSupportedException(
                "You must know either the lookup table or the Catalogue you want to configure it on");
    }

    public override string GetCommandHelp() =>
        "Tells RDMP that a table contains code/description mappings for one of the columns in your dataset and that you (may) want them linked in when extracting the dataset";

    public override void Execute()
    {
        base.Execute();

        var cata = _catalogueIfKnown;
        if (cata == null)
            try
            {
                //make sure they really wanted to do this?
                if (YesNo(GetLookupConfirmationText(), "Create Lookup"))
                {
                    //get them to pick a Catalogue the table provides descriptions for
                    if (!SelectOne(_lookupTableInfoIfKnown.Repository, out cata))
                        return;
                }
                else
                {
                    return;
                }
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show("Error creating Lookup", exception);
                return;
            }

        //they now deifnetly have a Catalogue!
        var t = Activator.Activate<LookupConfigurationUI, Catalogue>(cata);

        if (_lookupTableInfoIfKnown != null)
            t.SetLookupTableInfo(_lookupTableInfoIfKnown);
    }

    private string GetLookupConfirmationText() =>
        $@"You have chosen to make '{_lookupTableInfoIfKnown}' a Lookup Table (e.g T = Tayside, F=Fife etc).  In order to do this you will need to pick which Catalogue the column
provides a description for (a given TableInfo can be a Lookup for many columns in many datasets).";

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.Lookup, OverlayKind.Add);
}