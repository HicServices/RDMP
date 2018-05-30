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
    public class ExtractGlobalsCommand : ExtractCommand
    {
        public GlobalsBundle Globals { get; set; }

        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; private set; }
        public List<IExtractionResults> ExtractionResults { get; private set; }

        public ExtractGlobalsCommand(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IProject project, ExtractionConfiguration configuration, GlobalsBundle globals):base(configuration)
        {
            this.RepositoryLocator = repositoryLocator;
            this.Globals = globals;
            this.ExtractionResults = new List<IExtractionResults>();
        }

        public override DirectoryInfo GetExtractionDirectory()
        {
            return new ExtractionDirectory(Project.ExtractionDirectory, Configuration).GetGlobalsDirectory();
        }

        public override string DescribeExtractionImplementation()
        {
            return String.Join(";", Globals.Contents);
        }

    }
}