// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Security.Cryptography;

namespace ResearchDataManagementPlatform.WindowManagement.Licenses;

/// <summary>
/// Facilitates reading from the embedded license files for RDMP and third party libraries.  Also generates MD5 for tracking when a user has
/// agreed to a license that has been subsequently changed in a software update (e.g. if we use a new library).
/// </summary>
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
    public string GetHashOfLicense()
    {
        using (var hashProvider = SHA512.Create())
        {
            using (var stream = GetStream())
            {
                return BitConverter.ToString(hashProvider.ComputeHash(stream));
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
        var stream = typeof (License).Assembly.GetManifestResourceStream(_resourceFilename) ?? throw new Exception($"Could not find EmbeddedResource '{_resourceFilename}'");
        return stream;
    }
}