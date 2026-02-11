using MathNet.Numerics.RootFinding;
using Microsoft.EntityFrameworkCore;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Models;
using Rdmp.Core.Providers;
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

        public T[] GetAllObjects<T>() { 
            return null;//todo
        }

        public IEnumerable<IMapsDirectlyToDatabaseTable> GetAllObjects(Type t){
            return null;//todo
        }

        public T GetObjectByID<T>(int id)
        {
            throw new NotImplementedException();
        }
        public T[] GetAllObjectsWithParent<T>(object o ){ return null; }

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

            modelBuilder.Entity<Models.Catalogue>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Acronym).HasMaxLength(100);
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Acronym);
                entity.HasIndex(e => e.IsDeprecated);
       //         entity.Property(e => e.Type).HasConversion(v => v.ToString(),
       //     v => (CatalogueType)Enum.Parse(typeof(CatalogueType), v));

       //         entity.Property(e => e.Purpose).HasConversion(v => v.ToString(),
       //     v => (DatasetPurpose)Enum.Parse(typeof(DatasetPurpose), v));

       //         entity.Property(e => e.Periodicity).HasConversion(v => v.ToString(),
       // v => (CataloguePeriodicity)Enum.Parse(typeof(CataloguePeriodicity), v));

       //         entity.Property(e => e.Granularity).HasConversion(v => v.ToString(),
       // v => (CatalogueGranularity)Enum.Parse(typeof(CatalogueGranularity), v));

       //         entity.Property(e => e.Update_freq).HasConversion(v => v.ToString(),
       // v => (UpdateFrequencies)Enum.Parse(typeof(UpdateFrequencies), v));

       //         entity.Property(e => e.UpdateLag).HasConversion(v => v.ToString(),
       //v => (UpdateLagTimes)Enum.Parse(typeof(UpdateLagTimes), v));

            });
        }

        public CohortAggregateContainer GetParent(AggregateConfiguration aggregateConfiguration)
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

        public  IEnumerable<ExtendedProperty> GetExtendedProperties(string replacedBy)
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
            if(obj is FolderNode<Core.Models.Catalogue> fnc)
            {
                return Catalogues.Where(c => c.Folder == fnc.FullName);
            }
            if(obj is Models.Catalogue catalogue)
            {
                return CatalogueItems.Where(ci => ci.Catalogue.ID == catalogue.ID);
            }
            if(obj is Models.CatalogueItem catalogueItem)
            {
                return ColumnInfos.Where(ci => ci.CatalogueItems.Select(ci => ci.ID).Contains(catalogueItem.ID));
            }

            return new List<string>() { };
        }
    }

}

