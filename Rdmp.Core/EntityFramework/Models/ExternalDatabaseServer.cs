using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("ExternalDatabaseServer")]
    public class ExternalDatabaseServer
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [MaxLength(500)]
        public string Name { get; set; }

        [Required]
        public string Server { get; set; }

        public string Database { get; set; }
        public string DatabaseType { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

}
