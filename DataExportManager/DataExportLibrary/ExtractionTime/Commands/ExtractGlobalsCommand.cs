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
    /// In the Extraction use case, this Command represent the extraction of all Globals defined in an ExtractionConfiguration through an extraction pipeline.
    /// This includes SupportingDocuments and SuppotingSQL.
    /// </summary>
    public class ExtractGlobalsCommand : IExtractCommand
    {
        private readonly Project project;

        public GlobalsBundle Globals { get; set; }

        public ExtractCommandState State { get; set; }
        public string Name { get; private set; }
        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; private set; }
        public IExtractionConfiguration Configuration { get; set; }
        public List<IExtractionResults> ExtractionResults { get; private set; }

        public ExtractGlobalsCommand(IRDMPPlatformRepositoryServiceLocator repositoryLocator, Project project, ExtractionConfiguration configuration, GlobalsBundle globals)
        {
            this.RepositoryLocator = repositoryLocator;
            this.project = project;
            this.Configuration = configuration;
            this.Globals = globals;
            this.ExtractionResults = new List<IExtractionResults>();
        }

        public DirectoryInfo GetExtractionDirectory()
        {
            return new ExtractionDirectory(project.ExtractionDirectory, Configuration).GetGlobalsDirectory();
        }

        public string DescribeExtractionImplementation()
        {
            return String.Join(";", Globals.Contents);
        }

    }
}