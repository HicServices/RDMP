// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text.RegularExpressions;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     A reusable regular expression which is available system wide.  Use these to record important standardised concepts
///     which you need to use in RDMP.  For example if you have a
///     forbidlist for forbidden column names instead of copying and pasting the definition everywhere and into plugins etc
///     you can define it once in the catalogue database as a
///     StandardRegex with a description and then everyone can link against it and have access to a centralised
///     description.  This prevents you having multiple arguments getting out
///     of sync in Pipeline components for example.
/// </summary>
public class StandardRegex : DatabaseEntity, ICheckable
{
    /// <summary>
    ///     Reserved name for global parameter defining the Regex for ignoring columns in data load engine e.g. if you don't
    ///     want hic_ columns but instead want audit_ or something
    /// </summary>
    public const string DataLoadEngineGlobalIgnorePattern = "DataLoadEngineGlobalIgnorePattern";

    #region Database Properties

    private string _conceptName;
    private string _regex;
    private string _description;

    /// <summary>
    ///     Short human readable name for what the regex identifies e.g. 'chis'
    /// </summary>
    public string ConceptName
    {
        get => _conceptName;
        set => SetField(ref _conceptName, value);
    }

    /// <summary>
    ///     The string that is the Pattern of the Regex, the user can happily type in invalid stuff and it will not break until
    ///     it is used at runtime (so that we don't bust up at Design Time)
    /// </summary>
    public string Regex
    {
        get => _regex;
        set => SetField(ref _regex, value);
    }

    /// <summary>
    ///     Verbose user provided description of the Regex, history, purpose, how it works etc
    /// </summary>
    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    #endregion

    public StandardRegex()
    {
    }

    /// <summary>
    ///     Creates a new standardised reusable regular expression in the database that can be referenced by pipeline
    ///     components (this helps centralise patterns
    ///     rather than having replication between components/pipelines)
    /// </summary>
    /// <param name="repository"></param>
    public StandardRegex(ICatalogueRepository repository)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "ConceptName", $"New StandardRegex{Guid.NewGuid()}" },
            { "Regex", ".*" }
        });
    }

    internal StandardRegex(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        ConceptName = r["ConceptName"].ToString();
        Regex = r["Regex"].ToString();
        Description = r["Description"] as string;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return ConceptName;
    }

    public void Check(ICheckNotifier notifier)
    {
        if (string.IsNullOrWhiteSpace(Regex)) return;
        try
        {
            _ = new Regex(Regex);
            notifier.OnCheckPerformed(new CheckEventArgs("Regex is valid", CheckResult.Success));
        }
        catch (ArgumentException ex)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Regex is invalid", CheckResult.Fail, ex));
        }
    }
}