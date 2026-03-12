using MathNet.Numerics.RootFinding;
using Microsoft.EntityFrameworkCore;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.EntityFramework.Models;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Providers.Nodes.CohortNodes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rdmp.Core.Curation.Data.Catalogue;

namespace Rdmp.Core.EntityFramework
{
    public class RDMPDbContext : DbContext
    {

        public RDMPDbContext(DbContextOptions<RDMPDbContext> options)
           : base(options)
        { }

        public DbSet<Models.Catalogue> Catalogues { get; set; }
        public DbSet<Models.CatalogueItem> CatalogueItems { get; set; }
        public DbSet<Models.ColumnInfo> ColumnInfos { get; set; }

        public DbSet<Models.ExtractionInformation> ExtractionInformation { get; set; }

        public DbSet<Models.Dataset> Datasets { get; set; }
        public DbSet<Models.TableInfo> TableInfos { get; set; }
        public DbSet<Models.Lookup> Lookups{ get; set; }
        public DbSet<Models.LookupCompositeJoinInfo> LookupCompositeJoinInfos { get; set; }
        public DbSet<ANOTable> ANOTables { get; set; }
        public DbSet<Models.CohortIdentificationConfiguration> CohortIdentificationConfigurations { get; set; }

        public T[] GetAllObjects<T>()
        {
            return null;//todo
        }

        public IEnumerable<IMapsDirectlyToDatabaseTable> GetAllObjects(Type t)
        {
            return null;//todo
        }

        public T GetObjectByID<T>(int id)
        {
            throw new NotImplementedException();
        }
        public T[] GetAllObjectsWithParent<T>(object o) { return null; }

        public T[] GetReferencesTo<T>(object o) { return null; }//todo

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.CatalogueItem>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
                entity.HasIndex(e => e.Name);
                //entity.(e => e.Catalogue);
            });
            modelBuilder.Entity<Models.ColumnInfo>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
                entity.HasIndex(e => e.Name);
                //entity.HasIndex(e => e.CatalogueItem);
            });
            modelBuilder.Entity<Models.ExtractionInformation>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.SelectSQL).IsRequired();
                //entity.HasIndex(e => e.CatalogueItem);
            });

            modelBuilder.Entity<Models.Dataset>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired();
                //entity.HasIndex(e => e.CatalogueItem);
            });
            modelBuilder.Entity<Models.TableInfo>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired();
                //entity.HasIndex(e => e.CatalogueItem);
            });
            modelBuilder.Entity<Models.CohortIdentificationConfiguration>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            });
            modelBuilder.Entity<Models.Catalogue>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Acronym).HasMaxLength(100);
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Acronym);
                entity.HasIndex(e => e.IsDeprecated);
            });
        }

        public Curation.Data.Cohort.CohortAggregateContainer GetParent(AggregateConfiguration aggregateConfiguration)
        {
            throw new NotImplementedException();
        }

        public bool StillExists<T>(int allegedParent)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetAllObjectsWhere<T>(string v, int iD)
        {
            throw new NotImplementedException();
        }

        public IExternalDatabaseServer GetDefaultFor(PermissableDefaults liveLoggingServer_ID)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetAllObjectsInIDList<T>(List<int> list)
        {
            throw new NotImplementedException();
        }

        public T1[] GetAllObjectsWithParent<T1, T2>(T2 catalogue)
        {
            throw new NotImplementedException();
        }

        public CatalogueExtractabilityStatus GetExtractabilityStatus(Curation.Data.Catalogue catalogue)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AnyTableSqlParameter> GetAllParametersForParentTable(IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            throw new NotImplementedException();
        }

        public IMapsDirectlyToDatabaseTable GetObjectByID(Type type, int referencedObjectID)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IMapsDirectlyToDatabaseTable> GetAllObjectsInIDList(Type elementType, int[] ids)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ExtendedProperty> GetExtendedProperties(string replacedBy)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ExtendedProperty> GetExtendedProperties(string propertyName, IMapsDirectlyToDatabaseTable obj)
        {
            throw new NotImplementedException();
        }

        public bool SupportsObjectType(Type requiredType)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> SelectAllWhere<T>(string v1, string v2)
        {
            throw new NotImplementedException();
        }

        public DescendancyList GetDescendancyListIfAnyFor(object modelObject)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> GetChildren(object obj)
        {
            if (obj is FolderNode<Core.EntityFramework.Models.Catalogue> fnc)
            {
                return fnc.ChildFolders.Cast<object>().Union(fnc.ChildObjects);//Catalogues.Where(c => c.Folder == fnc.FullName);
            }
            if (obj is Models.Catalogue catalogue)
            {
                var catalogueItems = CatalogueItems.Where(ci => ci.Catalogue.ID == catalogue.ID).ToList();
                List<object> children = new();
                children.Add(new CatalogueItemsNode(catalogue, catalogueItems, ExtractionCategory.Core));
                children.Add(new CatalogueItemsNode(catalogue, catalogueItems, ExtractionCategory.Supplemental));
                children.Add(new CatalogueItemsNode(catalogue, catalogueItems, ExtractionCategory.SpecialApprovalRequired));
                children.Add(new CatalogueItemsNode(catalogue, catalogueItems, ExtractionCategory.Internal));
                children.Add(new CatalogueItemsNode(catalogue, catalogueItems, ExtractionCategory.Deprecated));
                children.Add(new CatalogueItemsNode(catalogue, catalogueItems, null));
                return children;
            }
            if(obj is CatalogueItemsNode cin)
            {
                return cin.CatalogueItems;
            }
            if (obj is Models.CatalogueItem catalogueItem)
            {
                var extractionInformations = ExtractionInformation.Where(ei => ei.CatalogueItem_ID == catalogueItem.ID).ToList();
                return [..ColumnInfos.Where(ci => ci.CatalogueItems.Select(ci => ci.ID).Contains(catalogueItem.ID)),..extractionInformations];
            }
            if (obj is FolderNode<Core.EntityFramework.Models.CohortIdentificationConfiguration> fcic)
            {
                return fcic.ChildFolders.Cast<object>().Union(fcic.ChildObjects);//Catalogues.Where(c => c.Folder == fnc.FullName);
            }
            if(obj is AllTemplateCohortIdentificationConfigurationsNode atcicn)
            {
                return CohortIdentificationConfigurations.Where(cic => cic.IsTemplate).ToList();
            }
            if (obj is AllOrphanAggregateConfigurationsNode aocicn)
            {
                return new List<string>() { }; ;// CohortIdentificationConfigurations.Where(cic => cic.IsTemplate).ToList();
            }
            if (obj is AllTemplateAggregateConfigurationsNode atacicn)
            {
                return new List<string>() { }; ;// CohortIdentificationConfigurations.Where(cic => cic.IsTemplate).ToList();
            }
            return new List<string>() { };
        }
    }

}

