using System;
using System.Data;
using System.IO;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using Sharing.CommandExecution;

namespace LoadModules.Generic.DataFlowOperations
{
    public class ExtractCatalogueMetadata : IPluginDataFlowComponent<DataTable>, IPipelineRequirement<IExtractCommand>
    {
        private IExtractCommand _request;

        [DemandsInitialization(@"How do you want to name datasets, use the following tokens if you need them:   
         $p - Project Name ('e.g. My Project')
         $n - Project Number (e.g. 234)
         $c - Configuration Name (e.g. 'Cases')
         $d - Dataset name (e.g. 'Prescribing')
         $a - Dataset acronym (e.g. 'Presc') 

         You must have either $a or $d
         THIS WILL OVERRIDE THE TableNamingPattern at the destination!
         ", Mandatory = true, DefaultValue = "$c_$d")]
        public string MetadataNamingPattern { get; set; }
        
        public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            toProcess.TableName = GetTableName();
            toProcess.ExtendedProperties.Add("ProperlyNamed", true);

            var extractDatasetCommand = _request as ExtractDatasetCommand;
            if (extractDatasetCommand != null)
            {
                var catalogue = extractDatasetCommand.Catalogue;
            
                var sourceFolder = _request.GetExtractionDirectory();
                if (sourceFolder == null)
                    throw new Exception("Could not find Source Folder. DOes the project have an Extraction Directory defined?");

                var outputFolder = sourceFolder.Parent.CreateSubdirectory(ExtractionDirectory.METADATA_FOLDER_NAME);
                var outputFile = new FileInfo(Path.Combine(outputFolder.FullName, toProcess.TableName + ".sd"));

                var cmd = new ExecuteCommandExportObjectsToFile(extractDatasetCommand.RepositoryLocator, catalogue, outputFile);
                cmd.Execute();
            }
            return toProcess;
        }

        public string GetTableName()
        {
            string tblName = MetadataNamingPattern;
            var project = _request.Configuration.Project;

            tblName = tblName.Replace("$p", project.Name);
            tblName = tblName.Replace("$n", project.ProjectNumber.ToString());
            tblName = tblName.Replace("$c", _request.Configuration.Name);

            if (_request is ExtractDatasetCommand)
            {
                tblName = tblName.Replace("$d", ((ExtractDatasetCommand)_request).DatasetBundle.DataSet.Catalogue.Name);
                tblName = tblName.Replace("$a", ((ExtractDatasetCommand)_request).DatasetBundle.DataSet.Catalogue.Acronym);
            }

            if (_request is ExtractGlobalsCommand)
            {
                tblName = tblName.Replace("$d", ExtractionDirectory.GLOBALS_DATA_NAME);
                tblName = tblName.Replace("$a", "G");
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
            
        }

        public void PreInitialize(IExtractCommand value, IDataLoadEventListener listener)
        {
            _request = value;
        }
    }
}
