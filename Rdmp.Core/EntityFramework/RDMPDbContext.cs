using Microsoft.EntityFrameworkCore;
using Rdmp.Core.Models;
using System;
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

        public DbSet<Catalogue> Catalogues { get; set; }


        public T[] GetAllObjects<T>() { 
            return null;//todo
        }

        internal T GetObjectByID<T>(int id)
        {
            throw new NotImplementedException();
        }
        public T[] GetAllObjectsWithParent<T>(object o ){ return null; }

        public T[] GetReferencesTo<T>(object o) { return null; }//todo

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Catalogue>(entity =>
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

    }

}

