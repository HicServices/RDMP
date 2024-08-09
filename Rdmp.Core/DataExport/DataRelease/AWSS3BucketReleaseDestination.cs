// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.
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
using System.IO;
using Rdmp.Core.DataExport.DataExtraction;
using Rdmp.Core.DataExport.DataRelease.Audit;

namespace Rdmp.Core.DataExport.DataRelease;

/// <summary>
/// Release engine Destination for writing data to an AWS S3 Bucket
/// </summary>
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

    [DemandsInitialization("If selected, AWS configuration will be asked for at runtime", defaultValue: false)]
    public bool ConfigureInteractivelyOnRelease { get; set; }

    private ReleaseData _releaseData;
    private Project _project;
    private AWSReleaseEngine _engine;

    private AWSS3 _s3Helper;
    private RegionEndpoint _region;
    private S3Bucket _bucket;
    private List<IExtractionConfiguration> _configurationReleased;

    private IBasicActivateItems _activator;


    public void Abort(IDataLoadEventListener listener)
    {

        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "This component cannot Abort!"));

    }

    public void Check(ICheckNotifier notifier)
    {
        ((ICheckable)ReleaseSettings).Check(notifier);
        //TODO make the aws stuff pop up if not configured
        if (ConfigureInteractivelyOnRelease && _activator is not null)
        {
            _activator.TypeText("Set AWS Region", "What AWS region is your bucket in?", 128, AWS_Region, out var newRegion, false);
            AWS_Region = newRegion;
        }
        if (string.IsNullOrWhiteSpace(AWS_Region))
        {
            notifier.OnCheckPerformed(new CheckEventArgs("No AWS Region Specified.", CheckResult.Fail));
            return;
        }
        _region = RegionEndpoint.GetBySystemName(AWS_Region);
        if (ConfigureInteractivelyOnRelease && _activator is not null)
        {
            _activator.TypeText("Set AWS Profile", "What AWS profile do you want to use?", 128, AWS_Profile, out var newProfile, false);
            AWS_Profile = newProfile;
        }
        if (string.IsNullOrWhiteSpace(AWS_Profile))
        {
            notifier.OnCheckPerformed(new CheckEventArgs("No AWS Profile Specified.", CheckResult.Fail));
            return;
        }

        _s3Helper = new AWSS3(AWS_Profile, _region);
        if (ConfigureInteractivelyOnRelease && _activator is not null)
        {
            _activator.TypeText("Set S3 Bucket", "What S3 Bucket do you want to use?", 128, BucketName, out var newBucket, false);
            BucketName = newBucket;
        }

        try
        {
            _bucket = Task.Run(async () => await _s3Helper.GetBucket(BucketName)).Result;
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs(e.Message, CheckResult.Fail));
            return;
        }
        if (ConfigureInteractivelyOnRelease && _activator is not null)
        {
            _activator.TypeText("Set Subdirectory", "What is the name of the subfolder you want to use?", 128, BucketFolder, out var newFolder, false);
            BucketFolder = newFolder;
        }
        if (_s3Helper.DoesObjectExists(!string.IsNullOrWhiteSpace(BucketFolder) ? $"{BucketFolder}/contents.txt" : "contents.txt", _bucket.BucketName).Result)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Bucket Folder Already exists", CheckResult.Fail));
            return;
        }


    }

    public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
        if (pipelineFailureExceptionIfAny != null && _releaseData != null)
            try
            {
                var remnantsDeleted = 0;

                foreach (ExtractionConfiguration configuration in _releaseData.ConfigurationsForRelease.Keys)
                    foreach (ReleaseLog remnant in configuration.ReleaseLog)
                    {
                        remnant.DeleteInDatabase();
                        remnantsDeleted++;
                    }

                if (remnantsDeleted > 0)
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                        $"Because release failed we are deleting ReleaseLogEntries, this resulted in {remnantsDeleted} deleted records, you will likely need to re-extract these datasets or retrieve them from the Release directory"));
            }
            catch (Exception e1)
            {
                listener.OnNotify(this,
                    new NotifyEventArgs(ProgressEventType.Error,
                        "Error occurred when trying to clean up remnant ReleaseLogEntries", e1));
            }

        if (pipelineFailureExceptionIfAny == null)
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                $"Data release succeeded into:{_bucket.BucketName}"));
            //mark configuration as released
            foreach (var config in _configurationReleased)
            {
                config.IsReleased = true;
                config.SaveToDatabase();
            }

            if (ReleaseSettings.DeleteFilesOnSuccess)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Cleaning up..."));
                ExtractionDirectory.CleanupExtractionDirectory(this, _project.ExtractionDirectory,
                    _configurationReleased, listener);
            }
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "All done!"));
        }
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
        _configurationReleased = _engine.ConfigurationsReleased;
        return null;
    }
}
