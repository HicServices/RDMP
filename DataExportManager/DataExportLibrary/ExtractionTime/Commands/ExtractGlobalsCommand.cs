using System;
using System.IO;
using System.Linq;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.UserPicks;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;

namespace DataExportLibrary.ExtractionTime.Commands
{
    public class ExtractGlobalsCommand : IExtractCommand
    {
        private readonly Project project;

        public GlobalsBundle Globals { get; set; }

        public ExtractCommandState State { get; set; }
        public string Name { get; private set; }
        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; private set; }
        public IExtractionConfiguration Configuration { get; set; }

        public ExtractGlobalsCommand(IRDMPPlatformRepositoryServiceLocator repositoryLocator, Project project, ExtractionConfiguration configuration, GlobalsBundle globals)
        {
            this.RepositoryLocator = repositoryLocator;
            this.project = project;
            this.Configuration = configuration;
            this.Globals = globals;
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