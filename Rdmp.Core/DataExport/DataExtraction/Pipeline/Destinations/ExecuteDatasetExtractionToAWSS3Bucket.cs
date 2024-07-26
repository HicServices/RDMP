// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.


using Amazon;
using Amazon.S3.Model;
using NPOI.SS.Formula.Functions;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.DataExtraction.FileOutputFormats;
using Rdmp.Core.DataExport.DataRelease.Pipeline;
using Rdmp.Core.DataExport.DataRelease.Potential;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.AWS;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations
{
    /// <summary>
    /// Writes files to a directory within an Amazon S3 Bucket
    /// </summary>
    public class ExecuteDatasetExtractionToAWSS3Bucket : ExtractionDestination
    {
        [DemandsInitialization("The local AWS profile you wish to use for the extraction")]
        public string AWS_Profile { get; set; }

        [DemandsInitialization("The name of the bucket you wish to write to")]
        public string BucketName { get; set; }

        [DemandsInitialization("The AWS Region you wish to use")]
        public string AWS_Region { get; set; }

        [DemandsInitialization("The folder withing the bucket you wish to put the files. I.e. /my/directory/")]
        public string LocationWithinBucket { get; set; }

        [DemandsInitialization("Replace the fileid it already exisist in the S3 bucket")]
        public bool ReplaceExistingFiles { get; set; } = false;

        [DemandsInitialization(
            "The number of decimal places to round floating point numbers to.  This only applies to data in the pipeline which is hard typed Float and not to string values",
            DemandType.Unspecified)]
        public int? RoundFloatsTo { get; internal set; }

        private FileOutputFormat _output;


        private AWSS3 _s3Helper;
        private RegionEndpoint _region;
        private S3Bucket _bucket;
        private bool _objectCreated = false;


        public ExecuteDatasetExtractionToAWSS3Bucket() : base(false) { }

        public override void Abort(IDataLoadEventListener listener)
        {
            try
            {
                _output.Close();
            }
            catch (Exception)
            { }
            if (OutputFile is not null)
                try
                {
                    File.Delete(OutputFile);
                }
                catch (Exception) { }
            if (_objectCreated && _s3Helper.ObjectExists(AWSS3.KeyGenerator(LocationWithinBucket, $"{GetFilename()}.csv"), _bucket.BucketName))
            {
                //delete object
                _s3Helper.DeleteObject(AWSS3.KeyGenerator(LocationWithinBucket, $"{GetFilename()}.csv"), _bucket.BucketName);
            }
        }


        public override void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            if (OutputFile is not null)
            {
                try
                {
                    File.Delete(OutputFile);
                }
                catch (Exception) { }
            }
        }

        public override string GetDestinationDescription()
        {
            return $"AWS({_region}).Bucket{_bucket.BucketName}/{AWSS3.KeyGenerator(LocationWithinBucket, $"{GetFilename()}.csv")}";
        }

        public override GlobalReleasePotential GetGlobalReleasabilityEvaluator(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ISupplementalExtractionResults globalResult, IMapsDirectlyToDatabaseTable globalToCheck)
        {
            //TODO this should check if the file exists on AWS....
            throw new NotImplementedException();
        }

        public override ReleasePotential GetReleasePotential(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ISelectedDataSets selectedDataSet)
        {
            throw new NotImplementedException();
        }

        public override FixedReleaseSource<ReleaseAudit> GetReleaseSource(ICatalogueRepository catalogueRepository)
        {
            throw new NotImplementedException();
        }

        protected override void Open(DataTable toProcess, IDataLoadEventListener job, GracefulCancellationToken cancellationToken)
        {
            if (_request.IsBatchResume)
            {
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Unable to handle batch processing at this time."));
                return;
            }

            _output.Open(_request.IsBatchResume);

            // write the headers for the file unless we are resuming
            if (!_request.IsBatchResume) _output.WriteHeaders(toProcess);
        }

        protected override async void PreInitializeImpl(IExtractCommand request, IDataLoadEventListener listener)
        {
            _region = RegionEndpoint.GetBySystemName(AWS_Region);
            _s3Helper = new AWSS3(AWS_Profile, _region);
            _bucket = await _s3Helper.GetBucket(BucketName);
            OutputFile = Path.Combine(Path.GetTempPath(), $"{GetFilename()}.csv");
            _output = request.Configuration != null
                ? new CSVOutputFormat(OutputFile, request.Configuration.Separator, DateFormat)
                : new CSVOutputFormat(OutputFile, ",", DateFormat);
        }

        protected override async void WriteRows(DataTable toProcess, IDataLoadEventListener job, GracefulCancellationToken cancellationToken, Stopwatch stopwatch)
        {
            foreach (DataRow row in toProcess.Rows)
            {
                _output.Append(row);

                LinesWritten++;

                if (LinesWritten % 1000 == 0)
                    job.OnProgress(this,
                        new ProgressEventArgs($"Write to file {OutputFile}",
                            new ProgressMeasurement(LinesWritten, ProgressType.Records), stopwatch.Elapsed));
            }

            job.OnProgress(this,
                new ProgressEventArgs($"Write to file {OutputFile}",
                    new ProgressMeasurement(LinesWritten, ProgressType.Records), stopwatch.Elapsed));
            _output.Close();
            job.OnNotify(this,
                      new NotifyEventArgs(ProgressEventType.Information, "Begining upload of file to AWS"));
            try
            {
                var result = await _s3Helper.PutObject(_bucket.BucketName, $"{GetFilename()}.csv", OutputFile, LocationWithinBucket);
                if (result == System.Net.HttpStatusCode.OK)
                {
                    _objectCreated = true;
                    job.OnNotify(this,
                           new NotifyEventArgs(ProgressEventType.Information, "Succesfully upload of file to AWS"));
                }
                else
                {
                    job.OnNotify(this,
                         new NotifyEventArgs(ProgressEventType.Error, $"Upload of file to AWS failed. Status Code: {result}"));
                }
            }
            catch (Exception ex)
            {
                job.OnNotify(this,
                        new NotifyEventArgs(ProgressEventType.Error, ex.Message));
            }
        }

        public override async void Check(ICheckNotifier notifier)
        {
            if (string.IsNullOrWhiteSpace(AWS_Profile))
            {
                notifier.OnCheckPerformed(new CheckEventArgs("AWS Profile is not set.", CheckResult.Fail));
                return;
            }
            _region = RegionEndpoint.GetBySystemName(AWS_Region);
            if (_region.DisplayName == "Unknown")
            {
                notifier.OnCheckPerformed(new CheckEventArgs($"Region {AWS_Region} is not valid.", CheckResult.Fail));
                return;
            }

            try
            {
                _s3Helper = new AWSS3(AWS_Profile, _region);
            }
            catch (Exception ex)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(ex.Message, CheckResult.Fail));
                return;
            }
            try
            {
                _bucket = await _s3Helper.GetBucket(BucketName);
            }
            catch (Exception)
            {
                notifier.OnCheckPerformed(new CheckEventArgs($"Bucket '{BucketName}' does not exist.", CheckResult.Fail));
                return;
            }
            var key = AWSS3.KeyGenerator(LocationWithinBucket, $"{GetFilename()}.csv");

            if (!ReplaceExistingFiles && _s3Helper.ObjectExists(key, _bucket.BucketName))
            {
                notifier.OnCheckPerformed(new CheckEventArgs($"{key} already exists. Please update configuration if you wish to overrwrite", CheckResult.Fail));
                return;
            }
            base.Check(notifier);
        }
    }
}
