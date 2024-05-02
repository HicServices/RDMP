// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Data.Common;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.QueryBuilding.SyntaxChecking;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.DataExport.Data;

/// <summary>
///     Stores parameter values for a DeployedExtractionFilter e.g. @healthboard = 'T'
/// </summary>
public class DeployedExtractionFilterParameter : DatabaseEntity, ISqlParameter
{
    #region Database Properties

    private int _extractionFilter_ID;
    private string _parameterSQL;
    private string _value;
    private string _comment;

    /// <summary>
    ///     The <see cref="ExtractionFilter" /> against which the parameter is declared.  The WHERE Sql of the filter should
    ///     reference this parameter (e.g. "[mydb]..[mytbl].[hb_extract] = @healthboard").
    /// </summary>
    [Relationship(typeof(DeployedExtractionFilter), RelationshipType.SharedObject)]
    public int ExtractionFilter_ID
    {
        get => _extractionFilter_ID;
        set => SetField(ref _extractionFilter_ID, value);
    }

    /// <inheritdoc />
    [Sql]
    public string ParameterSQL
    {
        get => _parameterSQL;
        set => SetField(ref _parameterSQL, value);
    }

    /// <inheritdoc />
    [Sql]
    public string Value
    {
        get => _value;
        set => SetField(ref _value, value);
    }

    /// <inheritdoc />
    public string Comment
    {
        get => _comment;
        set => SetField(ref _comment, value);
    }

    #endregion

    /// <inheritdoc />
    [NoMappingToDatabase]
    public string ParameterName => QuerySyntaxHelper.GetParameterNameFromDeclarationSQL(ParameterSQL);

    #region Relationships

    /// <inheritdoc cref="ExtractionFilter_ID" />
    [NoMappingToDatabase]
    public DeployedExtractionFilter ExtractionFilter =>
        Repository.GetObjectByID<DeployedExtractionFilter>(ExtractionFilter_ID);

    #endregion

    public DeployedExtractionFilterParameter()
    {
    }

    /// <summary>
    ///     Creates a new parameter in the metadata database.
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="parameterSQL">Declaration SQL for the parameter e.g. "DECLARE @bob as varchar(10)"</param>
    /// <param name="parent"></param>
    public DeployedExtractionFilterParameter(IDataExportRepository repository, string parameterSQL, IFilter parent)
    {
        Repository = repository;

        Repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "ParameterSQL", parameterSQL },
            { "ExtractionFilter_ID", parent.ID }
        });
    }

    /// <summary>
    ///     Reads an existing parameter out of the database
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="r"></param>
    internal DeployedExtractionFilterParameter(IDataExportRepository repository, DbDataReader r) : base(repository, r)
    {
        ExtractionFilter_ID = int.Parse(r["ExtractionFilter_ID"].ToString());
        ParameterSQL = r["ParameterSQL"] as string;
        Value = r["Value"] as string;
        Comment = r["Comment"] as string;
    }

    /// <summary>
    ///     returns the <see cref="ParameterName" />
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{ParameterName} = {Value}";
    }

    /// <summary>
    ///     Checks the parameter syntax (See <see cref="ParameterSyntaxChecker" />)
    /// </summary>
    /// <param name="notifier"></param>
    public void Check(ICheckNotifier notifier)
    {
        new ParameterSyntaxChecker(this).Check(notifier);
    }

    /// <inheritdoc />
    public IQuerySyntaxHelper GetQuerySyntaxHelper()
    {
        return ((DeployedExtractionFilter)GetOwnerIfAny()).GetQuerySyntaxHelper();
    }

    /// <inheritdoc />
    public IMapsDirectlyToDatabaseTable GetOwnerIfAny()
    {
        return Repository.GetObjectByID<DeployedExtractionFilter>(ExtractionFilter_ID);
    }

    public DeployedExtractionFilterParameter ShallowClone(DeployedExtractionFilter into)
    {
        var clone = new DeployedExtractionFilterParameter(DataExportRepository, ParameterSQL, into);
        CopyShallowValuesTo(clone);
        return clone;
    }
}