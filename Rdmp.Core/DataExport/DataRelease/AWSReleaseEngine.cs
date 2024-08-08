using Amazon.S3.Model;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataRelease.Pipeline;
using Rdmp.Core.DataExport.DataRelease.Potential;
using Rdmp.Core.ReusableLibraryCode.AWS;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.DataExport.DataRelease
{
    public class AWSReleaseEngine : ReleaseEngine
    {

        AWSS3 _s3Helper;
        S3Bucket _bucket;

        public AWSReleaseEngine(Project project, ReleaseEngineSettings settings, AWSS3 s3Helper, S3Bucket bucket, IDataLoadEventListener listener, ReleaseAudit releaseAudit) : base(project, settings, listener, releaseAudit)
        {
            _s3Helper = s3Helper;
            _bucket = bucket;
        }

        public override void DoRelease(Dictionary<IExtractionConfiguration, List<ReleasePotential>> toRelease, Dictionary<IExtractionConfiguration, ReleaseEnvironmentPotential> environments, bool isPatch)
        {

            //base.DoRelease(toRelease, environments, isPatch);   
            ConfigurationsToRelease = toRelease;
            var auditFilePath = Path.Combine(Path.GetTempPath(), "contents.txt");
            using (var sw = PrepareAuditFile(auditFilePath))
            {
                ReleaseGlobalFolder();
                // Audit Global Folder if there are any
                if (ReleaseAudit.SourceGlobalFolder != null)
                {
                    AuditDirectoryCreation(ReleaseAudit.SourceGlobalFolder.FullName, sw, 0);

                    foreach (var fileInfo in ReleaseAudit.SourceGlobalFolder.GetFiles())
                        AuditFileCreation(fileInfo.Name, sw, 1);
                }

                //ReleaseAllExtractionConfigurations(toRelease, sw, environments, isPatch);
                sw.Flush();
                sw.Close();
                //_bucket = Task.Run(async () => await _s3Helper.GetBucket(BucketName)).Result;

                Task.Run(async () => await _s3Helper.PutObject(_bucket.BucketName, "contents.txt", auditFilePath));
                File.Delete(auditFilePath);

            }
            ReleaseSuccessful = true;
        }

        protected void ReleaseGlobalFolder(DirectoryInfo directory = null)
        {
            if (directory == null)
                directory = ReleaseAudit.SourceGlobalFolder;

            if (ReleaseAudit.SourceGlobalFolder != null)
            {
                foreach(var dir in directory.GetDirectories())
                {
                    ReleaseGlobalFolder(dir);// todo this won't put it in the currect subfolder
                }
                foreach(var file in directory.EnumerateFiles())
                {
                    Task.Run(async ()=> await _s3Helper.PutObject(_bucket.BucketName, file.Name, file.FullName));
                }
            }
        }


        protected StreamWriter PrepareAuditFile(string path)
        {
            var sw = new StreamWriter(path);

            sw.WriteLine($"----------Details Of Release---------:{DateTime.Now}");
            sw.WriteLine($"ProjectName:{Project.Name}");
            sw.WriteLine($"ProjectNumber:{Project.ProjectNumber}");
            sw.WriteLine($"Project.ID:{Project.ID}");
            sw.WriteLine($"ThisFileWasCreated:{DateTime.Now}");

            sw.WriteLine($"----------Contents Of Directory---------:{DateTime.Now}");

            return sw;
        }
    }
}
