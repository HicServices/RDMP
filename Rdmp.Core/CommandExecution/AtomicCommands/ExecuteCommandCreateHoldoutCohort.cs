// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NPOI.OpenXmlFormats.Dml.Diagram;
using NPOI.XWPF.UserModel;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.ComponentModel;
using Terminal.Gui;

namespace Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands;

//Based off of ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration

public class ExecuteCommandCreateHoldoutCohort : CohortCreationCommandExecution
{
    private readonly CohortIdentificationConfiguration _cic;
    private IBasicActivateItems _activator;
    //private CohortIdentificationConfiguration CloneCreatedIfAny;
    private CohortIdentificationConfiguration HoldoutConfiguration;

    public ExecuteCommandCreateHoldoutCohort(IBasicActivateItems activator,
        CohortIdentificationConfiguration cic) : base(activator)
    {
        _activator = activator;
        _cic = cic;
    }

    public override string GetCommandHelp() => "TODO";
    

    public override string GetCommandName() => "Create Holdout Cohort";

    public override void Execute()
    {
        base.Execute();
        //this should give use the suer configuration for their hold out
        //we can fake it for now
        CohortHoldoutCreationRequest request = GetCohortHoldoutCreationRequest(_cic);

        if(request is null)
        {
            return;
        }


        //SELECT distinct top request.count percent
        //extraction_identifier, Rand()
        //FROM
        //data
        //where request.sql
        //group by extraction_identifier
        //ORDER BY RAND() desc


        //copy cohort
        HoldoutConfiguration = _cic.CreateClone(ThrowImmediatelyCheckNotifier.Quiet);
        CohortAggregateContainer container = HoldoutConfiguration.RootCohortAggregateContainer;


        CohortAggregateContainer subcontainer = new CohortAggregateContainer(_activator.RepositoryLocator.CatalogueRepository, SetOperation.INTERSECT);
        subcontainer.SaveToDatabase();
        container.AddChild(subcontainer);

        container.SaveToDatabase();
        //AggregateConfiguration[] aggregateConfirgurations = container.GetAggregateConfigurations();
        //foreach(AggregateConfiguration aggregate in aggregateConfirgurations)
        //{
        //    AggregateFilterContainer filterContainer = new AggregateFilterContainer(_activator.RepositoryLocator.CatalogueRepository, FilterContainerOperation.AND);
        //    var filter = new AggregateFilter(_activator.RepositoryLocator.CatalogueRepository, "name", filterContainer)
        //    {
        //        WhereSQL = ""
        //    };
        //    filter.SaveToDatabase();
        //    filterContainer.SaveToDatabase();
        //}
        //inject where
        //RootFilterContainer
        //var container = new AggregateFilterContainer(_activator.RepositoryLocator.CatalogueRepository, FilterContainerOperation.AND);
        //var filter = new AggregateFilter(_activator.RepositoryLocator.CatalogueRepository,"name",container )
        //{
        //    WhereSQL = ""
        //};
        //filter.SaveToDatabase();

        HoldoutConfiguration.SaveToDatabase();
        //todo add container ro aggregate configuration
        //inject top x %
        //savecohort
        //add cohhort as new excpet exisitng cohort builder query

    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.AllCohortsNode, OverlayKind.Add);
}