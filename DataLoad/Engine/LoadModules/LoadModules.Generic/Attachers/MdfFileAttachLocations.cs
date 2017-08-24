using System;
using System.IO;
using System.Linq;
using LoadModules.Generic.Exceptions;

namespace LoadModules.Generic.Attachers
{
    public class MdfFileAttachLocations
    {
        public MdfFileAttachLocations(DirectoryInfo originDirectory, string databaseDirectoryFromPerspectiveOfDatabaseServer, string copyToDirectoryOrNullIfDatabaseIsLocalhost)
        {
            if (databaseDirectoryFromPerspectiveOfDatabaseServer == null)
                throw new ArgumentNullException("databaseDirectoryFromPerspectiveOfDatabaseServer");

            var copyToDirectory = copyToDirectoryOrNullIfDatabaseIsLocalhost ?? databaseDirectoryFromPerspectiveOfDatabaseServer;

          
            var filesThatWeCouldLoad = originDirectory.GetFiles("*.mdf").ToArray();

            if(filesThatWeCouldLoad.Length == 0)
                throw new FileNotFoundException("Could not find any MDF files in the directory " + originDirectory.FullName);

            if(filesThatWeCouldLoad.Length > 1)
                throw new MultipleMatchingFilesException("Did not know which MDF file to attach, found multiple :" + string.Join(",",filesThatWeCouldLoad.Select(f=>f.Name)));

            OriginLocationMdf =filesThatWeCouldLoad[0].FullName;
       
            //veryify log file exists
            OriginLocationLdf = Path.Combine(Path.GetDirectoryName(OriginLocationMdf), Path.GetFileNameWithoutExtension(OriginLocationMdf) + "_log.ldf");

            if (!File.Exists(OriginLocationLdf))
                throw new FileNotFoundException("Cannot attach database, LOG file was not found:" + OriginLocationLdf, OriginLocationLdf);

            CopyToMdf = Path.Combine(copyToDirectory, Path.GetFileName(OriginLocationMdf));
            CopyToLdf = Path.Combine(copyToDirectory, Path.GetFileName(OriginLocationLdf));

            AttachMdfPath = Path.Combine(databaseDirectoryFromPerspectiveOfDatabaseServer,Path.GetFileName(OriginLocationMdf));
            AttachLdfPath = Path.Combine(databaseDirectoryFromPerspectiveOfDatabaseServer, Path.GetFileName(OriginLocationLdf));
        }

        public string OriginLocationMdf {get; set; }
        public string OriginLocationLdf { get; set; }

        public string CopyToMdf { get; set; }
        public string CopyToLdf { get; set; }

        public string AttachMdfPath { get; set; }
        public string AttachLdfPath { get; set; }
    }
}