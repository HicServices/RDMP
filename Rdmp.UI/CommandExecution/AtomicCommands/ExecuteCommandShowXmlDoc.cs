// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandShowXmlDoc : BasicUICommandExecution
{
    private readonly string _title;
    private readonly string _help;

    /// <summary>
    /// sets up the command to show xmldoc for the supplied <paramref name="classOrProperty"/>
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="classOrProperty">Name of a documented class/interface/property (e.g. "CohortIdentificationConfiguration.QueryCachingServer_ID")</param>
    /// <param name="title"></param>
    public ExecuteCommandShowXmlDoc(IActivateItems activator, string classOrProperty, string title) : base(activator)
    {
        _title = title;
        _help = activator.RepositoryLocator.CatalogueRepository.CommentStore.GetDocumentationIfExists(classOrProperty,
            true, true);

        if (string.IsNullOrWhiteSpace(_help))
            SetImpossible($"No help available for keyword '{classOrProperty}'");
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) => iconProvider.GetImage(RDMPConcept.Help);

    public override void Execute()
    {
        base.Execute();
        BasicActivator.Show(_title, _help);
    }
}