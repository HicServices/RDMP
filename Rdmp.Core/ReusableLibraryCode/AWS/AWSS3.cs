// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.


using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Rdmp.Core.ReusableLibraryCode.AWS;

/// <summary>
/// Helper Class to interact with AWS and S3 Buckets
/// </summary>
public sealed class AWSS3
{
    private readonly AmazonS3Client _client;

    public AWSS3(string profile, RegionEndpoint region)
    {
        var profile1 = profile ?? "default";
        var credentials = AWSCredentialsHelper.LoadSsoCredentials(profile1);
        var awsEndpoint = Environment.GetEnvironmentVariable("AWS_ENDPOINT_URL");
        if (awsEndpoint != null)
        {
            var config = new AmazonS3Config
            {
                ServiceURL = awsEndpoint,
                UseHttp = true,
                ForcePathStyle = true,
            };
            _client = new AmazonS3Client(credentials, config);
        }
        else
        {
            _client = new AmazonS3Client(credentials, region);
        }
    }

    public async Task<List<S3Bucket>> ListAvailableBuckets()
    {
        var foundBuckets = await _client.ListBucketsAsync();
        return foundBuckets.Buckets;
    }

    public async Task<S3Bucket> GetBucket(string bucketName)
    {
        var foundBuckets = await _client.ListBucketsAsync();
        var bucket = foundBuckets.Buckets.Single(bucket => bucket.BucketName == bucketName);
        if (bucket == null) throw new Exception("Bucket not found...");

        return bucket;
    }

    private static string KeyGenerator(string path, string file) => Path.Join(path, file).Replace("\\", "/");

    public bool ObjectExists(string fileKey, string bucketName)
    {
        try
        {
            var response = _client.GetObjectMetadataAsync(new GetObjectMetadataRequest
            {
                BucketName = bucketName,
                Key = fileKey
            });
            return response.Result is not null;
        }

        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    public void DeleteObject(string fileKey, string bucketName)
    {
        _client.DeleteObjectAsync(new DeleteObjectRequest()
        {
            BucketName = bucketName,
            Key = fileKey,
        });
    }
    public async Task<HttpStatusCode> PutObject(string bucketName, string objectName, string localFilePath, string bucketSubdirectory = null)
    {

        var key = objectName;
        if (bucketSubdirectory != null)
            key = KeyGenerator(bucketSubdirectory, key);
        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = key,
            FilePath = localFilePath,
        };
        var response = await _client.PutObjectAsync(request);
        return response.HttpStatusCode;
    }
}
