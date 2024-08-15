using NUnit.Framework;
using Minio;
using Tests.Common;
using Rdmp.Core.ReusableLibraryCode.AWS;
using System.Threading.Tasks;
using Amazon.Runtime;
using Tests.Common.Scenarios;
using FAnsi.Discovery;
using NUnit.Framework.Legacy;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Logging;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.Core.Tests.DataExport.DataExtraction;
using System.Data;
using System;
using System.IO;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.DataRelease.Pipeline;
using Rdmp.Core.DataExport.DataRelease;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System.Linq;
using NPOI.POIFS.Crypt;
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
        private readonly string endpoint = "127.0.0.1:9000";//"play.min.io";
        private static IMinioClient _minioClient;


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
            var pipe = new Pipeline(CatalogueRepository, "NestedPipe");
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
        public void LocationAlreadyExists()
        {
            MakeBucket("releasetoawsbasictest");

            var file = Path.GetTempFileName();
            using (var sw = new StreamWriter(file))
            {
                sw.WriteLine("Name,Surname,Age,Healthiness,DateOfImagining");
                sw.WriteLine("Frank,\"Mortus,M\",41,0.00,2005-12-01");
                sw.WriteLine("Bob,Balie,12,1,2013-06-11");
                sw.WriteLine("Munchen,'Smith',43,0.3,2002-01-01");
                sw.WriteLine("Carnage,Here there is,29,0.91,2005-01-01");
                sw.WriteLine("Nathan,Crumble,51,0.78,2005-01-01");
                sw.Close();
            }
            var opArgs = new PutObjectArgs().WithBucket("releasetoawsbasictest").WithObject("release").WithFileName(file);
            _minioClient.PutObjectAsync(opArgs);
            DoExtraction();
            var pipe = new Pipeline(CatalogueRepository, "NestedPipe");
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
            var ex = Assert.Throws<Exception>(() => runner.Run(RepositoryLocator, ThrowImmediatelyDataLoadEventListener.Quiet, ThrowImmediatelyCheckNotifier.Quiet, new GracefulCancellationToken()));
            var foundObjects = GetObjects("releasetoawsbasictest");
            Assert.That(foundObjects.Count, Is.EqualTo(1));
            DeleteBucket("releasetoawsbasictest");
        }

    }
}
