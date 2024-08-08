using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataRelease.Pipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataFlowPipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.CommandExecution;
using Amazon.S3.Model;
using Rdmp.Core.ReusableLibraryCode.AWS;
using Amazon;
using Org.BouncyCastle.Crypto.Fpe;
using System.IO;
using System.Globalization;

namespace Rdmp.Core.DataExport.DataRelease
{
    public class AWSS3BucketReleaseDestination : IPluginDataFlowComponent<ReleaseAudit>, IDataFlowDestination<ReleaseAudit>,
    IPipelineRequirement<Project>, IPipelineRequirement<ReleaseData>
    {

        [DemandsNestedInitialization] public ReleaseEngineSettings ReleaseSettings { get; set; }
        [DemandsInitialization("The local AWS profile you wish to use for the extraction")]
        public string AWS_Profile { get; set; }

        [DemandsInitialization("The name of the bucket you wish to write to")]
        public string BucketName { get; set; }

        [DemandsInitialization("The AWS Region you wish to use")]
        public string AWS_Region { get; set; }


        [DemandsInitialization("The folder in the S3 bucket you wish to release to", defaultValue: "")]
        public string BucketFolder { get; set; }

        private ReleaseData _releaseData;
        private Project _project;
        private AWSReleaseEngine _engine;
        private List<IExtractionConfiguration> _configurationReleased;
        private DirectoryInfo _destinationFolder;

        private IBasicActivateItems _activator;


        public void SetActivator(IBasicActivateItems activator)
        {
            _activator = activator;
        }


        private AWSS3 _s3Helper;
        private RegionEndpoint _region;
        private S3Bucket _bucket;

        public void Abort(IDataLoadEventListener listener)
        {
            throw new NotImplementedException();
        }

        public void Check(ICheckNotifier notifier)
        {
            ((ICheckable)ReleaseSettings).Check(notifier);
            //TODO make the aws stuff pop up if not configured
            if (string.IsNullOrWhiteSpace(AWS_Region))
            {
                notifier.OnCheckPerformed(new CheckEventArgs("No AWS Region Specified.", CheckResult.Fail));
                return;
            }
            _region = RegionEndpoint.GetBySystemName(AWS_Region);
            if (string.IsNullOrWhiteSpace(AWS_Profile))
            {
                notifier.OnCheckPerformed(new CheckEventArgs("No AWS Profile Specified.", CheckResult.Fail));
                return;
            }

            _s3Helper = new AWSS3(AWS_Profile, _region);
            try
            {
                _bucket = Task.Run(async () => await _s3Helper.GetBucket(BucketName)).Result;
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(e.Message, CheckResult.Fail));
                return;
            }
            //todo check location on bucket doesn't already exists
            //check if the folder exists
            if (_s3Helper.DoesObjectExists(!string.IsNullOrWhiteSpace(BucketFolder) ? $"{BucketFolder}/contents.txt" : "contents.txt", _bucket.BucketName).Result)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Bucket Folder Already exists", CheckResult.Fail));
                return;
            }


        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            throw new NotImplementedException();
        }

        public void PreInitialize(Project value, IDataLoadEventListener listener)
        {
            _project = value;
        }

        public void PreInitialize(ReleaseData value, IDataLoadEventListener listener)
        {
            _releaseData = value;
        }

        public ReleaseAudit ProcessPipelineData(ReleaseAudit releaseAudit, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if (releaseAudit == null)
                return null;
            _region = RegionEndpoint.GetBySystemName(AWS_Region);
            _s3Helper = new AWSS3(AWS_Profile, _region);
            _bucket = Task.Run(async () => await _s3Helper.GetBucket(BucketName)).Result;
            //if (releaseAudit.ReleaseFolder == null)
            //{
            //    if (string.IsNullOrWhiteSpace(BucketFolder))
            //    {
            //        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "No Release Folder or S3 Bucket path were specified. Files will be placed in the root of the S3 Bucket"));
            //    }
            //}
            if (_releaseData.ReleaseState == ReleaseState.DoingPatch)
            {
                //TODO this is untested, but a blind copy from the other release destination
                listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Information,
                    "CumulativeExtractionResults for datasets not included in the Patch will now be erased."));

                var recordsDeleted = 0;

                foreach (var (configuration, potentials) in _releaseData.ConfigurationsForRelease)
                    //foreach existing CumulativeExtractionResults if it is not included in the patch then it should be deleted
                    foreach (var redundantResult in configuration.CumulativeExtractionResults.Where(r =>
                                 potentials.All(rp => rp.DataSet.ID != r.ExtractableDataSet_ID)))
                    {
                        redundantResult.DeleteInDatabase();
                        recordsDeleted++;
                    }

                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                    $"Deleted {recordsDeleted} old CumulativeExtractionResults (That were not included in the final Patch you are preparing)"));

            }
            _region = RegionEndpoint.GetBySystemName(AWS_Region);
            _s3Helper = new AWSS3(AWS_Profile, _region);
            _engine = new AWSReleaseEngine(_project, ReleaseSettings, _s3Helper, _bucket, BucketFolder, listener, releaseAudit);
            _engine.DoRelease(_releaseData.ConfigurationsForRelease, _releaseData.EnvironmentPotentials,
                _releaseData.ReleaseState == ReleaseState.DoingPatch);

            _destinationFolder = _engine.ReleaseAudit.ReleaseFolder;
            _configurationReleased = _engine.ConfigurationsReleased;

            return null;
        }
    }
}
