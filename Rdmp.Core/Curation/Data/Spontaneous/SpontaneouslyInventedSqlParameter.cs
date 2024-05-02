// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.QueryBuilding.SyntaxChecking;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation.Data.Spontaneous;

/// <summary>
///     Spontaneous (memory only) implementation of ISqlParameter.  This class is used extensively when there is a need to
///     inject new ISqlParameters into an ISqlQueryBuilder
///     at runtime (or a ParameterManager).  The most common use case for this is merging two or more ISqlParameters that
///     have the exact same declaration/value into a single
///     new one (which will be SpontaneouslyInventedSqlParameter to prevent changes to the originals).
/// </summary>
public class SpontaneouslyInventedSqlParameter : SpontaneousObject, ISqlParameter
{
    private readonly IQuerySyntaxHelper _syntaxHelper;

    [Sql] public string ParameterSQL { get; set; }

    [Sql] public string Value { get; set; }

    public string Comment { get; set; }

    public SpontaneouslyInventedSqlParameter(MemoryRepository repo, string declarationSql, string value, string comment,
        IQuerySyntaxHelper syntaxHelper) : base(repo)
    {
        _syntaxHelper = syntaxHelper;
        ParameterSQL = declarationSql;
        Value = value;
        Comment = comment;
    }

    public string ParameterName => QuerySyntaxHelper.GetParameterNameFromDeclarationSQL(ParameterSQL);

    public IMapsDirectlyToDatabaseTable GetOwnerIfAny()
    {
        //I am my own owner! mwahahaha
        return this;
    }

    public IQuerySyntaxHelper GetQuerySyntaxHelper()
    {
        return _syntaxHelper;
    }

    public void Check(ICheckNotifier notifier)
    {
        new ParameterSyntaxChecker(this).Check(notifier);
    }
}