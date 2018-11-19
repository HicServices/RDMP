using System;
using System.IO;
using System.Security.Cryptography;

namespace ResearchDataManagementPlatform.WindowManagement.Licenses
{
    public class License
    {
        private readonly string _resourceFilename;
        private const string LicenseResourcePath = "ResearchDataManagementPlatform.WindowManagement.Licenses.";

        /// <summary>
        /// The local path to the license file resource within this assembly e.g. LICENSE / LIBRARYLICENSES
        /// </summary>
        /// <param name="resourceFilename"></param>
        public License(string resourceFilename = "LICENSE")
        {
            resourceFilename = LicenseResourcePath + resourceFilename;
            _resourceFilename = resourceFilename;
        }

        /// <summary>
        /// Computes an MD5 Hash of the current License text
        /// </summary>
        /// <returns></returns>
        public string GetMd5OfLicense()
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = GetStream())
                {
                    return BitConverter.ToString(md5.ComputeHash(stream));
                }
            }
        }

        /// <summary>
        /// Returns the current License text
        /// </summary>
        /// <returns></returns>
        public string GetLicenseText()
        {
            using (var stream = GetStream())
            {
                return new StreamReader(stream).ReadToEnd();
            }
        }

        private Stream GetStream()
        {
            var stream = typeof (License).Assembly.GetManifestResourceStream(_resourceFilename);

            if (stream == null)
                throw new Exception("Could not find EmbeddedResource '" + _resourceFilename + "'");

            return stream;
        }
    }
}