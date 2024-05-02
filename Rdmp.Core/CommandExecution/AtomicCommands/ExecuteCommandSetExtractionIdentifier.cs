// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Change which column is used to perform linkage against a cohort.  This command supports both changing the global
///     setting on a <see cref="Catalogue" />
///     or changing it only for a specific <see cref="ExtractionConfiguration" />
/// </summary>
public sealed class ExecuteCommandSetExtractionIdentifier : ExecuteCommandSetColumnSettingBase, IAtomicCommand
{
    /// <summary>
    ///     Change which column is the linkage identifier in a <see cref="Catalogue" /> either at a global level or for a
    ///     specific <paramref name="inConfiguration" />
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="catalogue"></param>
    /// <param name="inConfiguration"></param>
    /// <param name="column"></param>
    public ExecuteCommandSetExtractionIdentifier(IBasicActivateItems activator,
            [DemandsInitialization("The dataset you want to change the extraction identifier for")]
            ICatalogue catalogue,
            [DemandsInitialization(
                "Optional - The specific extraction you want the change made in or Null for the Catalogue itself (will affect all future extractions)")]
            IExtractionConfiguration inConfiguration,
            [DemandsInitialization(
                "Optional - The Column name(s) you want to select as the new linkage identifier(s).  Comma separate multiple entries if needed")]
            string column)
        // base class args
        : base(activator, catalogue, inConfiguration, column,
            "Set Extraction Identifier",
            "Extraction Identifier")
    {
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.ExtractableCohort, OverlayKind.Key);
    }

    public override string GetCommandHelp()
    {
        return "Change which column(s) contain the patient id / linkage column e.g. CHI";
    }

    protected override bool IsValidSelection(ConcreteColumn[] selected)
    {
        return selected is not { Length: > 1 } // if multiple selected warn user
               ||
               YesNo(
                   "Are you sure you want multiple linkable extraction identifier columns (most datasets only have 1 person ID column in them)?",
                   "Multiple IsExtractionIdentifier columns?");
    }

    protected override bool Getter(ConcreteColumn c)
    {
        return c.IsExtractionIdentifier;
    }

    protected override void Setter(ConcreteColumn c, bool newValue)
    {
        c.IsExtractionIdentifier = newValue;
    }
}