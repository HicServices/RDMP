using NUnit.Framework;
using Minio;
using Rdmp.Core.ReusableLibraryCode.AWS;
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
using Rdmp.Core.Curation.Data.DataLoad;

namespace Rdmp.Core.Tests.DataExport.DataRelease;

public sealed class S3BucketReleaseDestinationTests : TestsRequiringAnExtractionConfiguration
{
    private const string Username = "minioadmin";
    private const string Password = "minioadmin";
    private const string Endpoint = "127.0.0.1:9000";
    private static IMinioClient _minioClient;


    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _minioClient?.Dispose();
    }

    [OneTimeSetUp]
    public new void OneTimeSetUp()
    {
        _minioClient = new MinioClient()
            .WithEndpoint(Endpoint)
            .WithCredentials(Username, Password)
            .WithSSL(false)
            .Build();
    }

    private void DoExtraction()
    {
        SetUp();
        Execute(out _, out _, ThrowImmediatelyDataLoadEventListener.Quiet);
    }

    private static void MakeBucket(string name)
    {
        var mbArgs = new MakeBucketArgs()
            .WithBucket(name);
        _minioClient.MakeBucketAsync(mbArgs).Wait();
    }

    private static void DeleteBucket(string name)
    {
        var rbArgs = new RemoveBucketArgs()
            .WithBucket(name);
        _minioClient.RemoveBucketAsync(rbArgs).Wait();
    }

    private static List<Minio.DataModel.Item> GetObjects(string bucketName)
    {
        var loArgs = new ListObjectsArgs().WithBucket(bucketName);
        var x = _minioClient.ListObjectsEnumAsync(loArgs).ToListAsync();
        return x.IsCompleted ? x.Result : x.AsTask().Result;
    }

    private static void SetArgs(IArgument[] args, Dictionary<string, object> values)
    {
        foreach (var x in args)
        {
            if (!values.TryGetValue(x.Name, out var value) || x.GetValueAsSystemType()?.Equals(value) == true) continue;

            x.SetValue(value);
            x.SaveToDatabase();
        }
    }

    [Test]
    public void AWSLoginTest()
    {
        var awss3 = new AWSS3("minio", Amazon.RegionEndpoint.EUWest2);
        Assert.DoesNotThrow(() => MakeBucket("logintest"));
        Assert.DoesNotThrow(() => DeleteBucket("logintest"));
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

        SetArgs(args, new Dictionary<string, object>
        {
            { "AWS_Profile", "minio" },
            { "BucketName", "releasetoawsbasictest" },
            { "AWS_Region", "eu-west-2" },
            { "ConfigureInteractivelyOnRelease", false },
            { "BucketFolder", "release" }
        });

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
        Assert.That(foundObjects, Has.Count.EqualTo(1));
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

        SetArgs(args, new Dictionary<string, object>
        {
            { "AWS_Profile", "minio" },
            { "BucketName", "noregion" },
            { "ConfigureInteractivelyOnRelease", false },
            { "BucketFolder", "release" }
        });

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

        SetArgs(args, new Dictionary<string, object>
        {
            { "AWS_Region", "eu-west-2" },
            { "BucketName", "noprofile" },
            { "ConfigureInteractivelyOnRelease", false },
            { "BucketFolder", "release" }
        });

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

        SetArgs(args, new Dictionary<string, object>
        {
            { "AWS_Region", "eu-west-2" },
            { "AWS_Profile", "junk-profile" },
            { "BucketName", "badprofile" },
            { "ConfigureInteractivelyOnRelease", false },
            { "BucketFolder", "release" }
        });

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
        SetArgs(args, new Dictionary<string, object>
        {
            { "AWS_Region", "eu-west-2" },
            { "AWS_Profile", "minio" },
            { "ConfigureInteractivelyOnRelease", false },
            { "BucketFolder", "release" }
        });

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
        SetArgs(args, new Dictionary<string, object>
        {
            { "AWS_Region", "eu-west-2" },
            { "AWS_Profile", "minio" },
            { "BucketName", "doesNotExist" },
            { "ConfigureInteractivelyOnRelease", false },
            { "BucketFolder", "release" }
        });

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
        MakeBucket("locationalreadyexist");

        DoExtraction();
        var pipe = new Pipeline(CatalogueRepository, "NestedPipe7");
        var pc = new PipelineComponent(CatalogueRepository, pipe, typeof(AWSS3BucketReleaseDestination), -1,
            "AWS S3 Release");
        pc.SaveToDatabase();

        var args = pc.CreateArgumentsForClassIfNotExists<AWSS3BucketReleaseDestination>();

        Assert.That(pc.GetAllArguments().Any());
        SetArgs(args, new Dictionary<string, object>
        {
            { "AWS_Region", "eu-west-2" },
            { "AWS_Profile", "minio" },
            { "BucketName", "locationalreadyexist" },
            { "ConfigureInteractivelyOnRelease", false },
            { "BucketFolder", "release" }
        });

        pipe.DestinationPipelineComponent_ID = pc.ID;
        pipe.SaveToDatabase();
        var optsRelease = new ReleaseOptions
        {
            Configurations = _configuration.ID.ToString(),
            Pipeline = pipe.ID.ToString()
        };
        var runner = new ReleaseRunner(new ThrowImmediatelyActivator(RepositoryLocator), optsRelease);
        Assert.DoesNotThrow(() => runner.Run(RepositoryLocator, ThrowImmediatelyDataLoadEventListener.Quiet, ThrowImmediatelyCheckNotifier.Quiet, new GracefulCancellationToken()));
        var foundObjects = GetObjects("locationalreadyexist");
        Assert.That(foundObjects, Has.Count.EqualTo(1));
        DoExtraction();
        pipe = new Pipeline(CatalogueRepository, "NestedPipe8");
        pc = new PipelineComponent(CatalogueRepository, pipe, typeof(AWSS3BucketReleaseDestination), -1,
            "AWS S3 Release");
        pc.SaveToDatabase();

        args = pc.CreateArgumentsForClassIfNotExists<AWSS3BucketReleaseDestination>();

        Assert.That(pc.GetAllArguments().Any());
        SetArgs(args, new Dictionary<string, object>
        {
            { "AWS_Region", "eu-west-2" },
            { "AWS_Profile", "minio" },
            { "BucketName", "locationalreadyexist" },
            { "ConfigureInteractivelyOnRelease", false },
            { "BucketFolder", "release" }
        });

        pipe.DestinationPipelineComponent_ID = pc.ID;
        pipe.SaveToDatabase();
        optsRelease = new ReleaseOptions
        {
            Configurations = _configuration.ID.ToString(),
            Pipeline = pipe.ID.ToString(),
            Command = CommandLineActivity.check
        };
        runner = new ReleaseRunner(new ThrowImmediatelyActivator(RepositoryLocator), optsRelease);
        Assert.Throws<AggregateException>(() => runner.Run(RepositoryLocator, ThrowImmediatelyDataLoadEventListener.Quiet, ThrowImmediatelyCheckNotifier.Quiet, new GracefulCancellationToken()));
        foundObjects = GetObjects("locationalreadyexist");
        Assert.That(foundObjects, Has.Count.EqualTo(1));
    }
}