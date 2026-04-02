using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Microsoft.EntityFrameworkCore;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.EntityFramework.Models.DataExport;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Providers.Nodes.ProjectCohortNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.EntityFramework
{
    public class DataExportDbContext : DbContext
    {
        public DataExportDbContext(DbContextOptions<DataExportDbContext> options)
          : base(options)
        { }


        public DbSet<Models.DataExport.ExtractableDataSetPackage> ExtractableDataSetPackages { get; set; }
        public DbSet<Models.DataExport.Project> Projects{ get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.DataExport.ExtractableDataSetPackage>(entity =>
            {
                entity.HasKey(e => e.ID);
            });
            modelBuilder.Entity<Models.DataExport.Project>(entity =>
            {
                entity.HasKey(e => e.ID);
            });
        }

        public T[] GetAllObjects<T>()
        {
            return null;//todo
        }

        public IEnumerable<IMapsDirectlyToDatabaseTable> GetAllObjects(Type t)
        {
            return null;//todo
        }
        public IEnumerable<T> GetAllObjectsWhere<T>(string v, int iD)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> GetChildren(object obj)
        {
            if(obj is FolderNode<Project> fnp)
            {
                return fnp.ChildFolders.Cast<object>().Union(fnp.ChildObjects);
            }
            if(obj is Project p)
            {
                return new List<object>()
                {
                    new ProjectCohortsNode(p),
                    new ExtractionConfigurationsNode(p),
                    new ExtractionDirectoryNode(p),
                    new ProjectCataloguesNode(p)
                };
            }
            if(obj is ProjectCohortsNode pcn)
            {
                //return new List<object>() { 
                //    FolderHelper.BuildFolderTree()
                //};
            }
            return new List<string>() { };

        }


    }
}
