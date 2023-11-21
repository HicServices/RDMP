// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ExtractionUIs.JoinsAndLookups;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandLinkColumnInfoToDataSetUI : BasicUICommandExecution, IAtomicCommand
{
    private readonly ColumnInfo _columnInfo;
    private Dataset _selectedDataset;
    private IActivateItems _activateItems;

    public ExecuteCommandLinkColumnInfoToDataSetUI(IActivateItems activator, ColumnInfo columnInfo) : base(activator)
    {
        _columnInfo = columnInfo;
        _activateItems = activator;
    }

    public override string GetCommandHelp() =>
        "TODO";

    public override void Execute()
    {
        base.Execute();
        Dataset[] datasets = _activateItems.RepositoryLocator.CatalogueRepository.GetAllObjects<Dataset>();
        DialogArgs da =  new()
        {
            WindowTitle = "Link a dataset with this column",
            TaskDescription =
             "Select the Dataset that this column information came from"
        };
        _selectedDataset = SelectOne(da, datasets);
        var backfill = YesNo("Link all other columns that match the source table?","Do you want to link this dataset to all other columns that reference the same table as this column?");
        var cmd = new ExecuteCommandLinkColumnInfoToDataset(_activateItems,_columnInfo, _selectedDataset,backfill);
        cmd.Execute();


        //var cic = _cic ?? (CohortIdentificationConfiguration)BasicActivator.SelectOne("Select Cohort Builder Query",
        //    BasicActivator.GetAll<CohortIdentificationConfiguration>().ToArray());

        //var cata = _catalogueIfKnown;
        //if (cata == null)
        //    try
        //    {
        //        //make sure they really wanted to do this?
        //        if (YesNo(GetLookupConfirmationText(), "Create Lookup"))
        //        {
        //            //get them to pick a Catalogue the table provides descriptions for
        //            if (!SelectOne(_lookupTableInfoIfKnown.Repository, out cata))
        //                return;
        //        }
        //        else
        //        {
        //            return;
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        ExceptionViewer.Show("Error creating Lookup", exception);
        //        return;
        //    }

        ////they now deifnetly have a Catalogue!
        //var t = Activator.Activate<LookupConfigurationUI, Catalogue>(cata);

        //if (_lookupTableInfoIfKnown != null)
        //    t.SetLookupTableInfo(_lookupTableInfoIfKnown);
    }

    //public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
    //    iconProvider.GetImage(RDMPConcept.Lookup, OverlayKind.Add);
}