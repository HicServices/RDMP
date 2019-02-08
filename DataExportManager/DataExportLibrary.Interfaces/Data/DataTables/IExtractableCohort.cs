// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    /// <summary>
    /// See ExtractableCohort
    /// </summary>
    public interface IExtractableCohort :  IHasQuerySyntaxHelper, IMightBeDeprecated
    {
        int CountDistinct { get; }
        int ExternalCohortTable_ID { get; }
        int OriginID { get; }
        string OverrideReleaseIdentifierSQL { get; set; }
        IExternalCohortTable ExternalCohortTable { get; }

        DataTable FetchEntireCohort();
        string GetPrivateIdentifier(bool runtimeName = false);
        string GetReleaseIdentifier(bool runtimeName = false);
        DataTable GetReleaseIdentifierMap(IDataLoadEventListener listener);
        string WhereSQL();
        string GetFirstProCHIPrefix();
        IExternalCohortDefinitionData GetExternalData();
        string GetPrivateIdentifierDataType();
        string GetReleaseIdentifierDataType();

        DiscoveredDatabase GetDatabaseServer();
        void ReverseAnonymiseDataTable(DataTable toProcess, IDataLoadEventListener listener, bool allowCaching);
        
    }
}