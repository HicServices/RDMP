// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.


using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Azure;
using Renci.SshNet;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace Rdmp.Core.ReusableLibraryCode.AWS
{
    public class AWSS3
    {

        public readonly string Profile;
        public readonly RegionEndpoint Region;
        private readonly AWSCredentials _credentials;
        private readonly AmazonS3Client _client;

        public AWSS3(string profile, RegionEndpoint region)
        {
            Profile = profile ?? "default";
            Region = region;
            _credentials = AWSCredentialsHelper.LoadSsoCredentials(Profile);
            _client = new AmazonS3Client(_credentials, Region);
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
            if (bucket == null)
            {
                throw new Exception("Bucket not found...");
            }
            return bucket;
        }

        public static string KeyGenerator(string path, string file)
        {
            return Path.Join(path, file).Replace("\\", "/");//todo there is probably a better way to do this
        }


        public bool ObjectExists(string fileKey, string bucketName)
        {
            try
            {
                _ = _client.GetObjectMetadataAsync(new GetObjectMetadataRequest()
                {
                    BucketName = bucketName,
                    Key = fileKey
                });

                return true;
            }

            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return false;
                throw;
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
}
