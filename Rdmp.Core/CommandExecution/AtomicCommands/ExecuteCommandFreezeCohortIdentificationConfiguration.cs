// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandFreezeCohortIdentificationConfiguration : BasicCommandExecution
{
    private readonly CohortIdentificationConfiguration _cic;
    private readonly bool _desiredFreezeState;

    public ExecuteCommandFreezeCohortIdentificationConfiguration(IBasicActivateItems activator,
        CohortIdentificationConfiguration cic, bool desiredFreezeState) : base(activator)
    {
        _cic = cic;
        _desiredFreezeState = desiredFreezeState;
    }

    public override string GetCommandName()
    {
        return _desiredFreezeState ? "Freeze Configuration" : "Unfreeze Configuration";
    }

    public override void Execute()
    {
        base.Execute();

        if (_desiredFreezeState)
            _cic.Freeze();
        else
            _cic.Unfreeze();

        Publish(_cic);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return Image.Load<Rgba32>(CatalogueIcons.FrozenCohortIdentificationConfiguration);
    }
}