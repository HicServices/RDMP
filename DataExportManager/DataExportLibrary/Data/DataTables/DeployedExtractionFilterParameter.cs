// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Checks.SyntaxChecking;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.Data.DataTables
{
    /// <summary>
    /// Stores parameter values for a DeployedExtractionFilter
    /// </summary>
    public class DeployedExtractionFilterParameter: VersionedDatabaseEntity, ISqlParameter
    {
        #region Database Properties
        private int _extractionFilter_ID;
        private string _parameterSQL;
        private string _value;
        private string _comment;

        public int ExtractionFilter_ID
        {
            get { return _extractionFilter_ID; }
            set { SetField(ref _extractionFilter_ID, value); }
        }
        [Sql]
        public string ParameterSQL
        {
            get { return _parameterSQL; }
            set { SetField(ref _parameterSQL, value); }
        }
        [Sql]
        public string Value
        {
            get { return _value; }
            set { SetField(ref _value, value); }
        }
        public string Comment
        {
            get { return _comment; }
            set { SetField(ref _comment, value); }
        }
        #endregion

        //extracts the name ofthe parameter from the SQL
        [NoMappingToDatabase]
        public string ParameterName
        {
            get { return QuerySyntaxHelper.GetParameterNameFromDeclarationSQL(ParameterSQL); }
        }

        public DeployedExtractionFilterParameter(IDataExportRepository repository, string parameterSQL, IFilter parent)
        {
            Repository = repository;

            Repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"ParameterSQL", parameterSQL},
                {"ExtractionFilter_ID", parent.ID}
            });
        }

        internal DeployedExtractionFilterParameter(IDataExportRepository repository, DbDataReader r)
            : base(repository, r)
        {
            ExtractionFilter_ID = int.Parse(r["ExtractionFilter_ID"].ToString());
            ParameterSQL = r["ParameterSQL"] as string;
            Value = r["Value"] as string;
            Comment = r["Comment"] as string;
        }
       
        public override string ToString()
        {
            //return the name of the variable
            return ParameterName;
        }

        public void Check(ICheckNotifier notifier)
        {
            new ParameterSyntaxChecker(this).Check(notifier);
        }

        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            return ((DeployedExtractionFilter) GetOwnerIfAny()).GetQuerySyntaxHelper();
        }

        public IMapsDirectlyToDatabaseTable GetOwnerIfAny()
        {
            return Repository.GetObjectByID<DeployedExtractionFilter>(ExtractionFilter_ID);
        }
    }
}
