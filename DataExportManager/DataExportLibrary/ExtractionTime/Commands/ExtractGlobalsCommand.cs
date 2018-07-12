using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.UserPicks;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;

namespace DataExportLibrary.ExtractionTime.Commands
{
    /// <summary>
    /// Extraction command for the data export engine which mandates the extraction of all global (not dataset specific) files in an <see cref="ExtractionConfiguration"/> (e.g.
    /// <see cref="CatalogueLibrary.Data.SupportingSQLTable"/>)
    /// </summary>
    public class ExtractGlobalsCommand : ExtractCommand
    {
        private readonly IProject project;

        public GlobalsBundle Globals { get; set; }

        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; private set; }
        
        public List<IExtractionResults> ExtractionResults { get; private set; }

        public ExtractGlobalsCommand(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IProject project, ExtractionConfiguration configuration, GlobalsBundle globals):base(configuration)
        {
            this.RepositoryLocator = repositoryLocator;
            this.project = project;
            this.Globals = globals;

            ExtractionResults = new List<IExtractionResults>();
        }

        public override DirectoryInfo GetExtractionDirectory()
        {
            return new ExtractionDirectory(project.ExtractionDirectory, Configuration).GetGlobalsDirectory();
        }

        public override string DescribeExtractionImplementation()
        {
            return String.Join(";", Globals.Contents);
        }

        public override string ToString()
        {
            return ExtractionDirectory.GLOBALS_DATA_NAME;
        }
    }
}