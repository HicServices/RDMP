// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using FAnsi.Discovery;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataFlowPipeline.Events;
using Rdmp.Core.DataLoad.Engine.Pipeline;
using Rdmp.Core.DataLoad.Engine.Pipeline.Destinations;
using Rdmp.Core.Repositories;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.PipelineUIs.Pipelines;
using Rdmp.UI.SimpleDialogs.ForwardEngineering;


namespace Rdmp.UI.SimpleDialogs.SimpleFileImporting;

/// <summary>
/// Allows you to take data in a single data table and bulk insert it into a database (which you pick at the top of the screen).  You must select or create an appropriate pipeline.
/// This will consist of a source that is capable of reading your file (e.g. if the file is CSV use DelimitedFlatFileDataFlowSource) and zero or more middle components e.g. CleanStrings.
/// For destination your pipeline can have any destination that inherits from DataTableUploadDestination (this allows you to have custom plugin behaviour if you have some kind of
///  weird database repository).  After the pipeline has executed and your database has been populated with the data table then the ForwardEngineerCatalogue dialog will appear which
/// will let you create a Catalogue reference in the DataCatalogue database for the new table.  Note that this dialog should only be used for 'one off' or 'getting started' style
/// loads, if you plan to routinely load this data table then give it a LoadMetadata (See LoadMetadataUI).
/// </summary>
public partial class CreateNewCatalogueByImportingFileUI_Advanced : UserControl
{
    private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
    private readonly IActivateItems _activator;
    private readonly DiscoveredDatabase _database;
    private readonly bool _alsoForwardEngineerCatalogue;

    private FileInfo _file;
    private Project _projectSpecific;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ICatalogue CatalogueCreatedIfAny { get; private set; }

    public CreateNewCatalogueByImportingFileUI_Advanced(IActivateItems activator, DiscoveredDatabase database,
        FileInfo file, bool alsoForwardEngineerCatalogue, Project projectSpecific)
    {
        _repositoryLocator = activator.RepositoryLocator;
        _activator = activator;
        _database = database;
        _alsoForwardEngineerCatalogue = alsoForwardEngineerCatalogue;
        _projectSpecific = projectSpecific;

        InitializeComponent();

        configureAndExecutePipeline1 = new ConfigureAndExecutePipelineUI(
            ExecuteCommandCreateNewCatalogueByImportingFile.GetCreateCatalogueFromFileDialogArgs()
            , new UploadFileUseCase(file, database, activator), activator);
        _file = file;
        // 
        // configureAndExecutePipeline1
        // 
        configureAndExecutePipeline1.Dock = DockStyle.Fill;
        configureAndExecutePipeline1.Location = new System.Drawing.Point(0, 0);
        configureAndExecutePipeline1.Name = "configureAndExecutePipeline1";
        configureAndExecutePipeline1.Size = new System.Drawing.Size(979, 894);
        configureAndExecutePipeline1.TabIndex = 14;
        Controls.Add(configureAndExecutePipeline1);

        configureAndExecutePipeline1.PipelineExecutionFinishedsuccessfully +=
            ConfigureAndExecutePipeline1OnPipelineExecutionFinishedsuccessfully;
    }


    private void ConfigureAndExecutePipeline1OnPipelineExecutionFinishedsuccessfully(object sender,
        PipelineEngineEventArgs args)
    {
        //pipeline executed successfully
        if (_alsoForwardEngineerCatalogue)
        {
            string targetTable = null;

            try
            {
                var dest = (DataTableUploadDestination)args.PipelineEngine.DestinationObject;
                targetTable = dest.TargetTableName;
                var table = _database.ExpectTable(targetTable);

                var ui = new ConfigureCatalogueExtractabilityUI(_activator,
                    new TableInfoImporter(_repositoryLocator.CatalogueRepository, table),
                    $"File '{_file.FullName}'", _projectSpecific);
                ui.ShowDialog();

                var cata = CatalogueCreatedIfAny = ui.CatalogueCreatedIfAny;

                MessageBox.Show(cata != null
                    ? $"Catalogue {cata.Name} successfully created"
                    : "User cancelled Catalogue creation, data has been loaded and TableInfo/ColumnInfos exist in Data Catalogue but there will be no Catalogue");

                ParentForm.Close();
            }
            catch (Exception e)
            {
                ExceptionViewer.Show(
                    $"Failed to import TableInfo/Forward Engineer Catalogue from {_database}(Table was {targetTable ?? "Null!"}) - see Exception for details",
                    e);
            }
        }
    }
}