using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.Logging.PastEvents;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("LoadMetadata")]
    public class LoadMetadata : DatabaseObject, IHasFolder, ILoadMetadata, ILoggedActivityRootObject
    {
        [Key]
        public override int ID { get; set; }

        [Required]
        [MaxLength(500)]
        public string Name { get; set; }

        public string Description { get; set; }
        public string LocationOfForLoadingDirectory { get; set; }
        public string LocationOfForArchivingDirectory { get; set; }
        public string LocationOfExecutablesDirectory { get; set; }
        public string LocationOfCacheDirectory { get; set; }
        public bool IgnoreTrigger { get; set; }

        public virtual ICollection<ProcessTask> ProcessTasks { get; set; }
        public virtual ICollection<Catalogue> Catalogues { get; set; }
        public string Folder { get; set; }


        public virtual List<LoadMetadataCatalogueLinkage> LoadMetadataCatalogueLinkages { get; set; } = new();
        public virtual List<Core.EntityFramework.Models.LoadProgress> LoadProgresses { get; set; } = new();
        ILoadProgress[] ILoadMetadata.LoadProgresses => LoadProgresses.ToArray();

        public int? RootLoadMetadata_ID => throw new NotImplementedException();

        public Curation.Data.ExternalDatabaseServer OverrideRAWServer => throw new NotImplementedException();

        public bool AllowReservedPrefix { get; set; }

        IOrderedEnumerable<IProcessTask> ILoadMetadata.ProcessTasks => (IOrderedEnumerable<IProcessTask>)ProcessTasks;

        public DateTime? LastLoadTime { get; set; }

        [NotMapped]
        public string DefaultForLoadingPath = Path.Combine("Data", "ForLoading");
        [NotMapped]
        public string DefaultForArchivingPath = Path.Combine("Data", "ForArchiving");
        [NotMapped]
        public string DefaultExecutablesPath = "Executables";
        [NotMapped]
        public string DefaultCachePath = Path.Combine("Data", "Cache");

        public List<Catalogue> GetAllCatalogues()
        {
            return LoadMetadataCatalogueLinkages.Select(link => link.Catalogue).ToList();
        }

        public List<TableInfo> GetDistinctTableInfoList(bool includeLookups)
        {
            var toReturn = new List<TableInfo>();
            foreach (var catalogue in GetAllCatalogues())
            {
                foreach (var tableInfo in catalogue.GetTableInfoList(includeLookups))
                {
                    //if (!toReturn.Contains(tableInfo))
                    //    toReturn.Add(tableInfo);
                }
            }
            return toReturn;
        }
        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            var syntax = GetAllCatalogues().Select(c => c.GetQuerySyntaxHelper()).Distinct().ToArray();
            return syntax.Length > 1
                ? throw new Exception(
                    $"LoadMetadata '{this}' has multiple underlying Catalogue Live Database Type(s) - not allowed")
                : syntax.SingleOrDefault();
        }
        public override string ToString() => Name;

        public DirectoryInfo GetRootDirectory()
        {
            if (!string.IsNullOrWhiteSpace(LocationOfForLoadingDirectory) && !string.IsNullOrWhiteSpace(LocationOfForArchivingDirectory) && !string.IsNullOrWhiteSpace(LocationOfExecutablesDirectory) && !string.IsNullOrWhiteSpace(LocationOfCacheDirectory))
            {
                var forLoadingRoot = LocationOfForLoadingDirectory.Replace(DefaultForLoadingPath, "");
                var forArchivingRoot = LocationOfForArchivingDirectory.Replace(DefaultForArchivingPath, "");
                var forExecutablesRoot = LocationOfExecutablesDirectory.Replace(DefaultExecutablesPath, "");
                var forCacheRoot = LocationOfCacheDirectory.Replace(DefaultCachePath, "");
                if (forLoadingRoot == forArchivingRoot && forExecutablesRoot == forCacheRoot && forArchivingRoot == forExecutablesRoot)
                {
                    return new DirectoryInfo(forLoadingRoot);
                }
            }
            return null;
        }

        IEnumerable<ICatalogue> ILoadMetadata.GetAllCatalogues()
        {
            return GetAllCatalogues();
        }

        public DiscoveredServer GetDistinctLiveDatabaseServer()
        {
            throw new NotImplementedException();
        }

        public void RevertToDatabaseState()
        {
            throw new NotImplementedException();
        }

        public RevertableObjectReport HasLocalChanges()
        {
            throw new NotImplementedException();
        }

        public bool Exists()
        {
            throw new NotImplementedException();
        }

        public void SaveToDatabase()
        {
            throw new NotImplementedException();
        }

        public DiscoveredServer GetDistinctLoggingDatabase()
        {
            throw new NotImplementedException();
        }

        public DiscoveredServer GetDistinctLoggingDatabase(out IExternalDatabaseServer serverChosen)
        {
            throw new NotImplementedException();
        }

        public string GetDistinctLoggingTask()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ArchivalDataLoadInfo> FilterRuns(IEnumerable<ArchivalDataLoadInfo> runs)
        {
            throw new NotImplementedException();
        }
    }
}
