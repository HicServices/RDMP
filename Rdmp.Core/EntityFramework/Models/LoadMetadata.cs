using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rdmp.Core.Models
{
    [Table("LoadMetadata")]
    public class LoadMetadata
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [MaxLength(500)]
        public string Name { get; set; }

        public string Description { get; set; }
        public string LocationOfForLoadingDirectory { get; set; }
        public string LocationOfForArchivingDirectory { get; set; }
        public bool IgnoreTrigger { get; set; }

        public virtual ICollection<ProcessTask> ProcessTasks { get; set; }
        public virtual ICollection<Catalogue> Catalogues{ get; set; }
    }
}
