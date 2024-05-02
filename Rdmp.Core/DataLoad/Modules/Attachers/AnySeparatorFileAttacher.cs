// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.DataLoad.Modules.Attachers;

/// <summary>
///     Data load component for loading flat files into RAW tables (flat files must be delimited by a specific character
///     (or sequence) e.g. csv)
///     <para>
///         Allows you to load zero or more flat files which are delimited by a given character or sequence of characters.
///         For example comma
///         separated (use Separator ',') or Tab separated (Use Separator '\t').
///     </para>
/// </summary>
public class AnySeparatorFileAttacher : DelimitedFlatFileAttacher
{
    [DemandsInitialization(@"The file separator e.g. , for CSV.  For tabs type \t", Mandatory = true)]
    public string Separator
    {
        get => Source.Separator;
        set => Source.Separator = value;
    }

    public AnySeparatorFileAttacher() : base('A')
    {
    }

    public override void Check(ICheckNotifier notifier)
    {
        base.Check(notifier);

        //Do not use IsNullOrWhitespace because \t is whitespace! worse than that user might get it into his head to set separator to \t\t or something else horrendous
        if (Separator == null)
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    "Separator has not been set yet, this is the character or sequence which separates cells in your flat file.  For example in the case of a CSV (comma separated values) file the Separator argument should be set to ','",
                    CheckResult.Fail));
    }
}