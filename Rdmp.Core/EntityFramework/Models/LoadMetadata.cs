using Rdmp.Core.Curation.Data;
using Rdmp.Core.EntityFramework.Helpers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace Rdmp.Core.EntityFramework.Models
{
    [Table("LoadMetadata")]
    public class LoadMetadata : DatabaseObject, IHasFolder
    {
        [Key]
        public override int ID { get; set; }

        [Required]
        [MaxLength(500)]
        public string Name { get; set; }

        public string Description { get; set; }
        public string LocationOfForLoadingDirectory { get; set; }
        public string LocationOfForArchivingDirectory { get; set; }
        public string LocationOfExecutablesDirectory { get; set; }
        public string LocationOfCacheDirectory { get; set; }
        public bool IgnoreTrigger { get; set; }

        public virtual ICollection<ProcessTask> ProcessTasks { get; set; }
        public virtual ICollection<Catalogue> Catalogues { get; set; }
        public string Folder { get; set; }


        public List<LoadMetadataCatalogueLinkage> LoadMetadataCatalogueLinkages { get; set; }

        [NotMapped]
        public string DefaultForLoadingPath = Path.Combine("Data", "ForLoading");
        [NotMapped]
        public string DefaultForArchivingPath = Path.Combine("Data", "ForArchiving");
        [NotMapped]
        public string DefaultExecutablesPath = "Executables";
        [NotMapped]
        public string DefaultCachePath = Path.Combine("Data", "Cache");

        public override string ToString() => Name;

        public DirectoryInfo GetRootDirectory()
        {
            if (!string.IsNullOrWhiteSpace(LocationOfForLoadingDirectory) && !string.IsNullOrWhiteSpace(LocationOfForArchivingDirectory) && !string.IsNullOrWhiteSpace(LocationOfExecutablesDirectory) && !string.IsNullOrWhiteSpace(LocationOfCacheDirectory))
            {
                var forLoadingRoot = LocationOfForLoadingDirectory.Replace(DefaultForLoadingPath, "");
                var forArchivingRoot = LocationOfForArchivingDirectory.Replace(DefaultForArchivingPath, "");
                var forExecutablesRoot = LocationOfExecutablesDirectory.Replace(DefaultExecutablesPath, "");
                var forCacheRoot = LocationOfCacheDirectory.Replace(DefaultCachePath, "");
                if (forLoadingRoot == forArchivingRoot && forExecutablesRoot == forCacheRoot && forArchivingRoot == forExecutablesRoot)
                {
                    return new DirectoryInfo(forLoadingRoot);
                }
            }
            return null;
        }
    }
}
