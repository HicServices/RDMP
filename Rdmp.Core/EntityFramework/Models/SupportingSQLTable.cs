using FAnsi.Discovery;
using Rdmp.Core.EntityFramework.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("SupportingSQLTable")]
    public class SupportingSQLTable: DatabaseObject
    {
        public static string ExtractionFolderName { get; internal set; }
        [Key]
        public override int ID { get; set; }

        [Required]
        [MaxLength(500)]
        public string Name { get; set; }

        public int? Catalogue_ID { get; set; }
        public string SQL { get; set; }
        public bool Extractable { get; set; }
        public bool IsGlobal { get; set; }

        [ForeignKey("Catalogue_ID")]
        public virtual Catalogue Catalogue { get; set; }

        public int? ExternalDatabaseServer_ID { get; set; }

        [NotMapped]
        public ExternalDatabaseServer ExternalDatabaseServer => CatalogueDbContext.GetObjectByID<ExternalDatabaseServer>((int)ExternalDatabaseServer_ID);

        public string Ticket { get; set; }
        public object Description { get; set; }

        internal DiscoveredServer GetServer()
        {
            throw new NotImplementedException();
        }

        public enum FetchOptions
        {
            /// <summary>
            /// All resources
            /// </summary>
            AllGlobalsAndAllLocals,

            /// <summary>
            /// Global resources only
            /// </summary>
            AllGlobals,

            /// <summary>
            /// Non Global resources only
            /// </summary>
            AllLocals,

            /// <summary>
            /// Global resources only AND only if they are marked Extractable
            /// </summary>
            ExtractableGlobals,

            /// <summary>
            /// Non Global resources only AND only if they are marked Extractable
            /// </summary>
            ExtractableLocals,

            /// <summary>
            /// All resources that are marked Extractable
            /// </summary>
            ExtractableGlobalsAndLocals
        }
    }
}
