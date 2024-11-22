// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands.Sharing;

public abstract class ExecuteCommandImportShare : BasicCommandExecution
{
    private FileInfo _shareDefinitionFile;

    /// <summary>
    /// Sets up the base command to read ShareDefinitions from the selected <paramref name="sourceFileCollection"/> (pass null to have the user pick at Execute)
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="sourceFileCollection"></param>
    protected ExecuteCommandImportShare(IBasicActivateItems activator, FileCollectionCombineable sourceFileCollection) :
        base(activator)
    {
        if (sourceFileCollection != null)
        {
            if (!sourceFileCollection.IsShareDefinition)
                SetImpossible("Only ShareDefinition files can be imported");

            _shareDefinitionFile = sourceFileCollection.Files.Single();
        }
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        Image.Load<Rgba32>(FamFamFamIcons.page_white_get);

    public sealed override void Execute()
    {
        base.Execute();

        //ensure file selected
        if ((_shareDefinitionFile ??=
                BasicActivator.SelectFile("Select share definition file to import", "Share Definition", "*.sd")) ==
            null)
            return;

        var json = File.ReadAllText(_shareDefinitionFile.FullName);
        var shareManager = new ShareManager(BasicActivator.RepositoryLocator);

        var shareDefinitions = shareManager.GetShareDefinitionList(json);

        ExecuteImpl(shareManager, shareDefinitions);
    }

    protected abstract void ExecuteImpl(ShareManager shareManager, List<ShareDefinition> shareDefinitions);
}