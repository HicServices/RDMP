// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Reports;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
/// Extract metadata from one or more Catalogues based on a template file using string replacement e.g. $Name for the catalogue's name
/// </summary>
public class ExecuteCommandExtractMetadata : BasicCommandExecution
{
    private readonly Catalogue[] _catalogues;
    private readonly DirectoryInfo _outputDirectory;
    private readonly FileInfo _template;
    private readonly string _fileNaming;
    private readonly bool _oneFile;
    private readonly string _newlineSub;
    private readonly string _commaSub;

    public ExecuteCommandExtractMetadata(IBasicActivateItems basicActivator,
        Catalogue[] catalogues,
        [DemandsInitialization("Where new files should be generated")]
        DirectoryInfo outputDirectory,
        [DemandsInitialization(
            "Template file in which keys such as $Name will be replaced with the corresponding Catalogue entry")]
        FileInfo template,
        [DemandsInitialization(
            "How output files based on the template should be named.  Uses same replacement strategy as template contents e.g. $Name.xml")]
        string fileNaming,
        [DemandsInitialization(
            "True to append all outputs into a single file.  False to output a new file for every Catalogue")]
        bool oneFile,
        [DemandsInitialization(
            "Optional, specify a replacement for newlines when found in fields e.g. <br/>.  Leave as null to leave newlines intact.")]
        string newlineSub,
        [DemandsInitialization("Optional, specify a replacement for the token $Comma (defaults to ',')",
            DefaultValue = ",")]
        string commaSub = ",") : base(basicActivator)
    {
        _catalogues = catalogues;
        _outputDirectory = outputDirectory;
        _template = template;
        _fileNaming = fileNaming;
        _oneFile = oneFile;
        _newlineSub = newlineSub;
        _commaSub = commaSub;
    }

    public override void Execute()
    {
        base.Execute();

        var catas = _catalogues ?? BasicActivator
            .SelectMany("Which catalogues do you want to extract metadata for", typeof(Catalogue),
                BasicActivator.CoreChildProvider.AllCatalogues.Value);

        if (catas == null || !catas.Any())
            return;

        var outputDir = _outputDirectory ?? BasicActivator.SelectDirectory("Enter output directory");

        if (outputDir == null)
            return;

        var template = _template;

        if (template == null)
        {
            BasicActivator.Show("Pick a template");
            template = BasicActivator.SelectFile(
                "Enter metadata template (should have template values in it e.g. $Name, $Description etc)");
        }

        if (template == null)
            return;

        var fileNaming = _fileNaming;

        if (fileNaming == null)
            if (!BasicActivator.TypeText("File naming", "File Naming", 1000, $"$Name{template.Extension}",
                    out fileNaming, false))
                return;

        if (string.IsNullOrWhiteSpace(fileNaming))
            return;

        var reporter = new CustomMetadataReport(BasicActivator.RepositoryLocator)
        {
            NewlineSubstitution = _newlineSub,
            CommaSubstitution = _commaSub
        };
        reporter.GenerateReport(catas.Cast<Catalogue>().OrderBy(c => c.Name).ToArray(), outputDir, template, fileNaming,
            _oneFile);
    }
}