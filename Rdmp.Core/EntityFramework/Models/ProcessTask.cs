using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("ProcessTask")]
    public class ProcessTask
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int LoadMetadata_ID { get; set; }

        public string Path { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public int ProcessTaskType { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public int LoadStage { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Order { get; set; }

        [Required]
        public bool IsDisabled { get; set; }

        [ForeignKey("LoadMetadata_ID")]
        public virtual LoadMetadata LoadMetadata { get; set; }

        public virtual ICollection<ProcessTaskArgument> ProcessTaskArguments { get; set; }

    }
}
