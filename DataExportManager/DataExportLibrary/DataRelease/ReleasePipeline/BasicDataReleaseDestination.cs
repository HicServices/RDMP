using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Interfaces.Data.DataTables;
using Microsoft.Office.Interop.Word;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.DataRelease.ReleasePipeline
{
    public class BasicDataReleaseDestination:IPluginDataFlowComponent<FileInfo[]>,IDataFlowDestination<FileInfo[]>, IPipelineRequirement<IProject>
    {
        private IProject _project;
        private ZipArchive _zip;
        
        [DemandsInitialization("Delete the released files from the origin location if release is succesful",defaultValue:true)]
        public bool DeleteFilesOnSuccess { get; set; }

        [DemandsInitialization("Output folder")]
        public string OutputBaseFolder { get; set; }

        private readonly List<FileInfo> _processedFiles = new List<FileInfo>();
        private string _zipLocation;

        public FileInfo[] ProcessPipelineData(FileInfo[] toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            OpenZipFileIfNotOpen();

            foreach (FileInfo f in toProcess)
            {
                _zip.CreateEntryFromFile(f.FullName, f.Name);
                _processedFiles.Add(f);
            }

            return toProcess;
        }

        private void OpenZipFileIfNotOpen()
        {
            if(_zip != null)
                return;

            var basePath = String.IsNullOrWhiteSpace(OutputBaseFolder)
                                ? _project.ExtractionDirectory
                                : OutputBaseFolder;

            //_project.MasterTicket + "-" + 

            _zipLocation = Path.Combine(basePath, DateTime.Now.ToString("yyMMddhhmmss") + ".zip");
            _zip = ZipFile.Open(_zipLocation, ZipArchiveMode.Create);
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            if(pipelineFailureExceptionIfAny == null)
            {
                if(DeleteFilesOnSuccess)
                    foreach (var file in _processedFiles)
                    {
                        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to cleanup '" + file.Name + "'"));
                        file.Delete();
                    }

                _zip.Dispose();
            }
            else
            {
                if(_zip != null)
                    try
                    {
                        File.Delete(_zipLocation);
                        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Deleted '" + _zipLocation + "'"));
                    }
                    catch (Exception e)
                    {
                        listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Error, "Failed to delete file '" + _zipLocation +  "'",e));
                    }
            }
        }

        public void Abort(IDataLoadEventListener listener)
        {
            throw new NotImplementedException();
        }

        public void Check(ICheckNotifier notifier)
        {
        }

        public void PreInitialize(IProject value, IDataLoadEventListener listener)
        {
            _project = value;
        }
    }
}