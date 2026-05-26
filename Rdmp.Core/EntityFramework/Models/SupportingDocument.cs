using Rdmp.Core.EntityFramework.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("SupportingDocument")]
    public class SupportingDocument: DatabaseObject
    {
        [Key]
        public override int ID { get; set; }

        [Required]
        [MaxLength(500)]
        public string Name { get; set; }

        public int? Catalogue_ID { get; set; }
        public string URL { get; set; }
        public bool Extractable { get; set; }
        public bool IsGlobal { get; set; }

        [ForeignKey("Catalogue_ID")]
        public virtual Catalogue Catalogue { get; set; }

        internal FileInfo GetFileName()
        {
            throw new NotImplementedException();
        }

        internal bool IsReleasable()
        {
            throw new NotImplementedException();
        }
    }
}
