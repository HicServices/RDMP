// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using Rdmp.Core.Curation.DataHelper.RegexRedaction;
using System.Text.RegularExpressions;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandAddNewRegexRedactionConfiguration : BasicCommandExecution, IAtomicCommand
{

    private readonly string _name;
    private readonly string _redactionPattern;
    private readonly string _redactionString;
    private readonly string _description;
    private readonly IBasicActivateItems _activator;


    public ExecuteCommandAddNewRegexRedactionConfiguration(IBasicActivateItems activator, [DemandsInitialization("Name")] string name, [DemandsInitialization("pattern")] string redactionPattern, [DemandsInitialization("redaction")] string redactionString, string description = null) : base(activator)
    {
        _activator = activator;
        _name = name;
        _redactionPattern = redactionPattern;
        _redactionString = redactionString;
        _description = description;
    }

    public override void Execute()
    {
        base.Execute();
        var config = new RegexRedactionConfiguration(_activator.RepositoryLocator.CatalogueRepository, _name, new Regex(_redactionPattern), _redactionString, _description);
        config.SaveToDatabase();
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
      iconProvider.GetImage(RDMPConcept.RegexRedaction, OverlayKind.Add);
}
