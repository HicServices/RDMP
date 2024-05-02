// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.DataLoad;

/// <summary>
///     Final code a Data Load exits with
/// </summary>
public enum ExitCodeType
{
    /// <summary>
    ///     The load was succesful, there should be new/updated rows in the live database
    /// </summary>
    Success,

    /// <summary>
    ///     The load failed, no new data should be in live (Due to the RAW=>STAGING=>LIVE containment system).
    /// </summary>
    Error,

    /// <summary>
    ///     The load was cancelled mid way through by the user or a load component in an unexpected manner
    /// </summary>
    Abort,


    /// <summary>
    ///     The load was ended mid way through by a load component which decided the load wasn't required after all (e.g. an
    ///     FTP
    ///     server was empty).  This is considered to be a clean shutdown and not an error.
    /// </summary>
    OperationNotRequired
}