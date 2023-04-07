// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands.Sharing;

public class ExecuteCommandImportFilterDescriptionsFromShare : ExecuteCommandImportShare
{
    private readonly IFilter _toPopulate;

    [UseWithObjectConstructor]
    public ExecuteCommandImportFilterDescriptionsFromShare(IBasicActivateItems activator, IFilter toPopulate, FileInfo file) 
        : this(activator, toPopulate, new FileCollectionCombineable(new[] { file}))
    {

    }
    public ExecuteCommandImportFilterDescriptionsFromShare(IBasicActivateItems activator, IFilter toPopulate, FileCollectionCombineable cmd = null) : base(activator, cmd)
    {
        _toPopulate = toPopulate;

        if (!string.IsNullOrWhiteSpace(_toPopulate.WhereSQL) || !string.IsNullOrWhiteSpace(_toPopulate.Description) || _toPopulate.GetAllParameters().Any())
            SetImpossible("Filter is not empty (import requires a new blank filter)");
    }

    protected override void ExecuteImpl(ShareManager shareManager, List<ShareDefinition> shareDefinitions)
    {
        var definitionToImport = shareDefinitions.First();
        if (!typeof(IFilter).IsAssignableFrom(definitionToImport.Type))
            throw new Exception("ShareDefinition was not for an IFilter");

        var props = definitionToImport.Properties;

        //We could be crossing Type boundaries here e.g. importing an ExtractionFilter to overwrite an AggregateFilter so we don't want to use ImportPropertiesOnly
        _toPopulate.Name = props["Name"].ToString();
        _toPopulate.Description = (string)props["Description"];
        _toPopulate.WhereSQL = (string)props["WhereSQL"];
        _toPopulate.IsMandatory = (bool)props["IsMandatory"];

        var factory = _toPopulate.GetFilterFactory();

        foreach (var param in shareDefinitions.Skip(1))
        {
            if (!typeof(ISqlParameter).IsAssignableFrom(param.Type))
                throw new Exception("Expected ShareDefinition to start with 1 IFilter then have 0+ ISqlParameters instead we found a " + param.Type);

            var paramProps = param.Properties;

            var newParam = factory.CreateNewParameter(_toPopulate, (string)paramProps["ParameterSQL"]);
            newParam.Comment = (string)paramProps["Comment"];
            newParam.Value = (string)paramProps["Value"];

            newParam.SaveToDatabase();
        }

        _toPopulate.SaveToDatabase();
        Publish((DatabaseEntity)_toPopulate);
    }
}