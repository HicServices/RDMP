using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("ProcessTaskArgument")]
    public class ProcessTaskArgument
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int ProcessTask_ID { get; set; }
        [Required]
        public string Name { get; set; }
        public string Value { get; set; }

        [Required]
        public string Type { get; set; }
        public string Description { get; set; }

        [ForeignKey("ProcessTask_ID")]
        public virtual ProcessTask ProcessTask{ get; set; }
    }
}
