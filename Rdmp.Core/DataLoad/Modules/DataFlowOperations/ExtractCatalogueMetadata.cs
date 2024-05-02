// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.IO;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.DataExtraction;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.DataFlowOperations;

/// <summary>
///     Extraction component that will generate share definition files for the catalogues involved in the extraction.
///     <para>The Metadata Naming Pattern will also override the table name in the DataTable flow object.</para>
/// </summary>
public class ExtractCatalogueMetadata : IPluginDataFlowComponent<DataTable>, IPipelineRequirement<IExtractCommand>,
    IPipelineRequirement<IBasicActivateItems>
{
    private IExtractCommand _request;
    private IBasicActivateItems _activator;

    [DemandsInitialization(@"How do you want to name datasets, use the following tokens if you need them:   
         $p - Project Name ('e.g. My Project')
         $n - Project Number (e.g. 234)
         $c - Configuration Name (e.g. 'Cases')
         $d - Dataset name (e.g. 'Prescribing')
         $a - Dataset acronym (e.g. 'Presc') 

         You must have either $a or $d
         THIS WILL OVERRIDE THE TableNamingPattern at the destination!
         ", Mandatory = true, DefaultValue = "$d")]
    public string MetadataNamingPattern { get; set; }

    public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        toProcess.TableName = GetTableName();
        toProcess.ExtendedProperties.Add("ProperlyNamed", true);

        if (_request is ExtractDatasetCommand extractDatasetCommand)
        {
            var catalogue = extractDatasetCommand.Catalogue;

            var sourceFolder = _request.GetExtractionDirectory() ??
                               throw new Exception(
                                   "Could not find Source Folder. Does the project have an Extraction Directory defined?");
            var outputFolder = sourceFolder.Parent.CreateSubdirectory(ExtractionDirectory.METADATA_FOLDER_NAME);
            var outputFile = new FileInfo(Path.Combine(outputFolder.FullName, $"{toProcess.TableName}.sd"));

            catalogue.Name = toProcess.TableName;
            var cmd = new ExecuteCommandExportObjectsToFile(_activator, catalogue, outputFile);
            cmd.Execute();
            catalogue.RevertToDatabaseState();
        }

        return toProcess;
    }

    public string GetTableName()
    {
        var tblName = MetadataNamingPattern;
        var project = _request.Configuration.Project;

        tblName = tblName.Replace("$p", project.Name);
        tblName = tblName.Replace("$n", project.ProjectNumber.ToString());
        tblName = tblName.Replace("$c", _request.Configuration.Name);

        switch (_request)
        {
            case ExtractDatasetCommand extractDatasetCommand:
                tblName = tblName.Replace("$d", extractDatasetCommand.DatasetBundle.DataSet.Catalogue.Name);
                tblName = tblName.Replace("$a", extractDatasetCommand.DatasetBundle.DataSet.Catalogue.Acronym);
                break;
            case ExtractGlobalsCommand:
                tblName = tblName.Replace("$d", ExtractionDirectory.GLOBALS_DATA_NAME);
                tblName = tblName.Replace("$a", "G");
                break;
        }

        return tblName;
    }

    public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
    }

    public void Abort(IDataLoadEventListener listener)
    {
    }

    public void Check(ICheckNotifier notifier)
    {
        if (MetadataNamingPattern != null && MetadataNamingPattern.Contains("$a"))
            if (_request is ExtractDatasetCommand dsRequest && string.IsNullOrWhiteSpace(dsRequest.Catalogue.Acronym))
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Catalogue '{dsRequest.Catalogue}' does not have an Acronym but MetadataNamingPattern contains $a",
                    CheckResult.Fail));
    }

    public void PreInitialize(IExtractCommand value, IDataLoadEventListener listener)
    {
        _request = value;
    }

    public void PreInitialize(IBasicActivateItems value, IDataLoadEventListener listener)
    {
        _activator = value;
    }
}