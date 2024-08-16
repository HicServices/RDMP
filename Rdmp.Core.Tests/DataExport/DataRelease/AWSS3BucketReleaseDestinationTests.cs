using NUnit.Framework;
using Minio;
using Rdmp.Core.ReusableLibraryCode.AWS;
using System.Threading.Tasks;
using Tests.Common.Scenarios;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.DataRelease;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System.Linq;
using Minio.DataModel.Args;
using System.Collections.Generic;
using Minio.DataModel;

namespace Rdmp.Core.Tests.DataExport.DataRelease
{
    public class AWSS3BucketReleaseDestinationTests : TestsRequiringAnExtractionConfiguration
    {
        private readonly string APILocation = "http://172.17.0.2:9000 ";
        private readonly string username = "minioadmin";
        private readonly string password = "minioadmin";
        private readonly string endpoint = "127.0.0.1:9000";
        private static IMinioClient _minioClient;


        [TearDown]
        public void TearDown()
        {
            if (_minioClient is not null)
                _minioClient.Dispose();
        }

        [SetUp]
        public void Setup()
        {
            _minioClient = new MinioClient()
                                    .WithEndpoint(endpoint)
                                    .WithCredentials(username, password)
                                    .WithSSL(false)
                                    .Build();
        }

        private void DoExtraction()
        {
            base.SetUp();
            base.Execute(out var usecase, out var results, ThrowImmediatelyDataLoadEventListener.Quiet);
        }

        private void MakeBucket(string name)
        {
            var mbArgs = new MakeBucketArgs()
                       .WithBucket(name);
            Task.Run(async () => await _minioClient.MakeBucketAsync(mbArgs)).Wait();
        }

        private void DeleteBucket(string name)
        {
            var rbArgs = new RemoveBucketArgs()
                       .WithBucket(name);
            Task.Run(async () => await _minioClient.RemoveBucketAsync(rbArgs)).Wait();
        }

        private List<Item> GetObjects(string bucketName)
        {
            var loArgs = new ListObjectsArgs().WithBucket(bucketName);
            var x =  _minioClient.ListObjectsEnumAsync(loArgs);
            return Task.Run(async () => await x.ToListAsync()).Result as List<Item> ;
        }


        [Test]
        public void AWSLoginTest()
        {
            var awss3 = new AWSS3("minio", Amazon.RegionEndpoint.EUWest2);
            Assert.That(Task.Run(async () => await awss3.ListAvailableBuckets()).Result.Count, Is.EqualTo(0));
        }

        [Test]
        public void ReleaseToAWSBasicTest()
        {
            MakeBucket("releasetoawsbasictest");
            DoExtraction();
            var pipe = new Pipeline(CatalogueRepository, "NestedPipe1");
            var pc = new PipelineComponent(CatalogueRepository, pipe, typeof(AWSS3BucketReleaseDestination), -1,
                "AWS S3 Release");
            pc.SaveToDatabase();

            var args = pc.CreateArgumentsForClassIfNotExists<AWSS3BucketReleaseDestination>();

            Assert.That(pc.GetAllArguments().Any());

            var match = args.Single(a => a.Name == "AWS_Profile");
            match.SetValue("minio");
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "BucketName");
            match.SetValue("releasetoawsbasictest");
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "AWS_Region");
            match.SetValue("eu-west-2");
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "ConfigureInteractivelyOnRelease");
            match.SetValue(false);
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "BucketFolder");
            match.SetValue("release");
            match.SaveToDatabase();

            pipe.DestinationPipelineComponent_ID = pc.ID;
            pipe.SaveToDatabase();
            var optsRelease = new ReleaseOptions
            {
                Configurations = _configuration.ID.ToString(),
                Pipeline = pipe.ID.ToString()
            };
            var runner = new ReleaseRunner(new ThrowImmediatelyActivator(RepositoryLocator), optsRelease);
            Assert.DoesNotThrow(()=>runner.Run(RepositoryLocator, ThrowImmediatelyDataLoadEventListener.Quiet, ThrowImmediatelyCheckNotifier.Quiet, new GracefulCancellationToken()));
            var foundObjects = GetObjects("releasetoawsbasictest");
            Assert.That(foundObjects.Count, Is.EqualTo(1));
            DeleteBucket("releasetoawsbasictest");
        }

        [Test]
        public void NoRegion()
        {
            DoExtraction();
            var pipe = new Pipeline(CatalogueRepository, "NestedPipe2");
            var pc = new PipelineComponent(CatalogueRepository, pipe, typeof(AWSS3BucketReleaseDestination), -1,
                "AWS S3 Release");
            pc.SaveToDatabase();

            var args = pc.CreateArgumentsForClassIfNotExists<AWSS3BucketReleaseDestination>();

            Assert.That(pc.GetAllArguments().Any());

            var match = args.Single(a => a.Name == "AWS_Profile");
            match.SetValue("minio");
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "BucketName");
            match.SetValue("releasetoawsbasictest");
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "ConfigureInteractivelyOnRelease");
            match.SetValue(false);
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "BucketFolder");
            match.SetValue("release");
            match.SaveToDatabase();

            pipe.DestinationPipelineComponent_ID = pc.ID;
            pipe.SaveToDatabase();
            var optsRelease = new ReleaseOptions
            {
                Configurations = _configuration.ID.ToString(),
                Pipeline = pipe.ID.ToString(),
                Command = CommandLineActivity.check
            };
            var runner = new ReleaseRunner(new ThrowImmediatelyActivator(RepositoryLocator), optsRelease);
            Assert.Throws<AggregateException>(() => runner.Run(RepositoryLocator, ThrowImmediatelyDataLoadEventListener.Quiet, ThrowImmediatelyCheckNotifier.Quiet, new GracefulCancellationToken()));

        }

        [Test]
        public void NoProfile()
        {
            DoExtraction();
            var pipe = new Pipeline(CatalogueRepository, "NestedPipe3");
            var pc = new PipelineComponent(CatalogueRepository, pipe, typeof(AWSS3BucketReleaseDestination), -1,
                "AWS S3 Release");
            pc.SaveToDatabase();

            var args = pc.CreateArgumentsForClassIfNotExists<AWSS3BucketReleaseDestination>();

            Assert.That(pc.GetAllArguments().Any());

            var match = args.Single(a => a.Name == "AWS_Region");
            match.SetValue("eu-west-2");
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "BucketName");
            match.SetValue("releasetoawsbasictest");
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "ConfigureInteractivelyOnRelease");
            match.SetValue(false);
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "BucketFolder");
            match.SetValue("release");
            match.SaveToDatabase();

            pipe.DestinationPipelineComponent_ID = pc.ID;
            pipe.SaveToDatabase();
            var optsRelease = new ReleaseOptions
            {
                Configurations = _configuration.ID.ToString(),
                Pipeline = pipe.ID.ToString(),
                Command = CommandLineActivity.check
            };
            var runner = new ReleaseRunner(new ThrowImmediatelyActivator(RepositoryLocator), optsRelease);
            Assert.Throws<AggregateException>(() => runner.Run(RepositoryLocator, ThrowImmediatelyDataLoadEventListener.Quiet, ThrowImmediatelyCheckNotifier.Quiet, new GracefulCancellationToken()));

        }

        [Test]
        public void BadProfile()
        {
            DoExtraction();
            var pipe = new Pipeline(CatalogueRepository, "NestedPipe4");
            var pc = new PipelineComponent(CatalogueRepository, pipe, typeof(AWSS3BucketReleaseDestination), -1,
                "AWS S3 Release");
            pc.SaveToDatabase();

            var args = pc.CreateArgumentsForClassIfNotExists<AWSS3BucketReleaseDestination>();

            Assert.That(pc.GetAllArguments().Any());

            var match = args.Single(a => a.Name == "AWS_Region");
            match.SetValue("eu-west-2");
            match = args.Single(a => a.Name == "AWS_Profile");
            match.SetValue("junk-profile");
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "BucketName");
            match.SetValue("releasetoawsbasictest");
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "ConfigureInteractivelyOnRelease");
            match.SetValue(false);
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "BucketFolder");
            match.SetValue("release");
            match.SaveToDatabase();

            pipe.DestinationPipelineComponent_ID = pc.ID;
            pipe.SaveToDatabase();
            var optsRelease = new ReleaseOptions
            {
                Configurations = _configuration.ID.ToString(),
                Pipeline = pipe.ID.ToString(),
                Command = CommandLineActivity.check
            };
            var runner = new ReleaseRunner(new ThrowImmediatelyActivator(RepositoryLocator), optsRelease);
            Assert.Throws<AggregateException>(() => runner.Run(RepositoryLocator, ThrowImmediatelyDataLoadEventListener.Quiet, ThrowImmediatelyCheckNotifier.Quiet, new GracefulCancellationToken()));

        }


        [Test]
        public void NoBucket()
        {
            DoExtraction();
            var pipe = new Pipeline(CatalogueRepository, "NestedPipe5");
            var pc = new PipelineComponent(CatalogueRepository, pipe, typeof(AWSS3BucketReleaseDestination), -1,
                "AWS S3 Release");
            pc.SaveToDatabase();

            var args = pc.CreateArgumentsForClassIfNotExists<AWSS3BucketReleaseDestination>();

            Assert.That(pc.GetAllArguments().Any());

            var match = args.Single(a => a.Name == "AWS_Region");
            match.SetValue("eu-west-2");
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "AWS_Profile");
            match.SetValue("minio");
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "ConfigureInteractivelyOnRelease");
            match.SetValue(false);
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "BucketFolder");
            match.SetValue("release");
            match.SaveToDatabase();

            pipe.DestinationPipelineComponent_ID = pc.ID;
            pipe.SaveToDatabase();
            var optsRelease = new ReleaseOptions
            {
                Configurations = _configuration.ID.ToString(),
                Pipeline = pipe.ID.ToString(),
                Command = CommandLineActivity.check
            };
            var runner = new ReleaseRunner(new ThrowImmediatelyActivator(RepositoryLocator), optsRelease);
            Assert.Throws<AggregateException>(() => runner.Run(RepositoryLocator, ThrowImmediatelyDataLoadEventListener.Quiet, ThrowImmediatelyCheckNotifier.Quiet, new GracefulCancellationToken()));

        }

        [Test]
        public void BadBucket()
        {
            DoExtraction();
            var pipe = new Pipeline(CatalogueRepository, "NestedPipe6");
            var pc = new PipelineComponent(CatalogueRepository, pipe, typeof(AWSS3BucketReleaseDestination), -1,
                "AWS S3 Release");
            pc.SaveToDatabase();

            var args = pc.CreateArgumentsForClassIfNotExists<AWSS3BucketReleaseDestination>();

            Assert.That(pc.GetAllArguments().Any());

            var match = args.Single(a => a.Name == "AWS_Region");
            match.SetValue("eu-west-2");
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "AWS_Profile");
            match.SetValue("minio");
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "BucketName");
            match.SetValue("doesNotExist");
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "ConfigureInteractivelyOnRelease");
            match.SetValue(false);
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "BucketFolder");
            match.SetValue("release");
            match.SaveToDatabase();

            pipe.DestinationPipelineComponent_ID = pc.ID;
            pipe.SaveToDatabase();
            var optsRelease = new ReleaseOptions
            {
                Configurations = _configuration.ID.ToString(),
                Pipeline = pipe.ID.ToString(),
                Command = CommandLineActivity.check
            };
            var runner = new ReleaseRunner(new ThrowImmediatelyActivator(RepositoryLocator), optsRelease);
            Assert.Throws<AggregateException>(() => runner.Run(RepositoryLocator, ThrowImmediatelyDataLoadEventListener.Quiet, ThrowImmediatelyCheckNotifier.Quiet, new GracefulCancellationToken()));

        }


        [Test]
        public void LocationAlreadyExists()
        {
            MakeBucket("releasetoawsbasictest");

            DoExtraction();
            var pipe = new Pipeline(CatalogueRepository, "NestedPipe7");
            var pc = new PipelineComponent(CatalogueRepository, pipe, typeof(AWSS3BucketReleaseDestination), -1,
                "AWS S3 Release");
            pc.SaveToDatabase();

            var args = pc.CreateArgumentsForClassIfNotExists<AWSS3BucketReleaseDestination>();

            Assert.That(pc.GetAllArguments().Any());

            var match = args.Single(a => a.Name == "AWS_Profile");
            match.SetValue("minio");
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "BucketName");
            match.SetValue("releasetoawsbasictest");
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "AWS_Region");
            match.SetValue("eu-west-2");
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "ConfigureInteractivelyOnRelease");
            match.SetValue(false);
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "BucketFolder");
            match.SetValue("release");
            match.SaveToDatabase();

            pipe.DestinationPipelineComponent_ID = pc.ID;
            pipe.SaveToDatabase();
            var optsRelease = new ReleaseOptions
            {
                Configurations = _configuration.ID.ToString(),
                Pipeline = pipe.ID.ToString()
            };
            var runner = new ReleaseRunner(new ThrowImmediatelyActivator(RepositoryLocator), optsRelease);
            Assert.DoesNotThrow(() => runner.Run(RepositoryLocator, ThrowImmediatelyDataLoadEventListener.Quiet, ThrowImmediatelyCheckNotifier.Quiet, new GracefulCancellationToken()));
            var foundObjects = GetObjects("releasetoawsbasictest");
            Assert.That(foundObjects.Count, Is.EqualTo(1));
            DoExtraction();
            pipe = new Pipeline(CatalogueRepository, "NestedPipe8");
            pc = new PipelineComponent(CatalogueRepository, pipe, typeof(AWSS3BucketReleaseDestination), -1,
                "AWS S3 Release");
            pc.SaveToDatabase();

             args = pc.CreateArgumentsForClassIfNotExists<AWSS3BucketReleaseDestination>();

            Assert.That(pc.GetAllArguments().Any());

             match = args.Single(a => a.Name == "AWS_Profile");
            match.SetValue("minio");
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "BucketName");
            match.SetValue("releasetoawsbasictest");
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "AWS_Region");
            match.SetValue("eu-west-2");
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "ConfigureInteractivelyOnRelease");
            match.SetValue(false);
            match.SaveToDatabase();
            match = args.Single(a => a.Name == "BucketFolder");
            match.SetValue("release");
            match.SaveToDatabase();

            pipe.DestinationPipelineComponent_ID = pc.ID;
            pipe.SaveToDatabase();
            optsRelease = new ReleaseOptions
            {
                Configurations = _configuration.ID.ToString(),
                Pipeline = pipe.ID.ToString(),
                Command = CommandLineActivity.check

            };
            runner = new ReleaseRunner(new ThrowImmediatelyActivator(RepositoryLocator), optsRelease);
            var ex = Assert.Throws<AggregateException>(() => runner.Run(RepositoryLocator, ThrowImmediatelyDataLoadEventListener.Quiet, ThrowImmediatelyCheckNotifier.Quiet, new GracefulCancellationToken()));
            foundObjects = GetObjects("releasetoawsbasictest");
            Assert.That(foundObjects.Count, Is.EqualTo(1));
            DeleteBucket("releasetoawsbasictest");
        }

    }
}
