// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.DataExport.Data;

/// <summary>
///     Provides the salt which will be passed for use by the Hashing algorithm in data extraction  (See
///     ConfigureHashingAlgorithm).
/// </summary>
public interface IHICProjectSalt
{
    /// <summary>
    ///     Returns a salt for hashing.  If you want reproducible extraction outputs then this should be based on an attribute
    ///     of
    ///     the <see cref="IProject" /> being extracted (or the <see cref="IExtractionConfiguration" />).
    /// </summary>
    /// <returns></returns>
    string GetSalt();
}