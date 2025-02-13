// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Exceptions;

namespace Rdmp.Core.Curation.Checks;

/// <summary>
/// Checks whether a given IFilter which was known to be cloned is still identical to its parent ExtractionFilter.  Also confirms that its parent still exists.
/// It is legal to modify an IFilter after cloning into a subfilter (e.g. AggregateFilter) therefore this class only provides Warning level events
/// </summary>
public class ClonedFilterChecker : ICheckable
{
    private readonly IFilter _child;
    private int? _allegedParent;
    private readonly IRepository _catalogueDatabaseRepository;

    /// <summary>
    /// Prepares to check the supplied IFilter which must be of a lower filter type to its parent ExtractionFilter
    /// </summary>
    /// <param name="child"></param>
    /// <param name="allegedParentExtractionFilterID">ExtractionFilter from which the IFilter was derived</param>
    /// <param name="catalogueDatabaseRepository"></param>
    public ClonedFilterChecker(IFilter child, int? allegedParentExtractionFilterID,
        IRepository catalogueDatabaseRepository)
    {
        _child = child;
        _allegedParent = allegedParentExtractionFilterID;
        _catalogueDatabaseRepository = catalogueDatabaseRepository;
    }

    /// <summary>
    /// Checks that the alleged parent of the cloned IFilter still exists and issues a warning if the SQL has changed vs the master (might not
    ///  be a problem since user customisation is allowed)
    /// </summary>
    /// <param name="notifier"></param>
    public void Check(ICheckNotifier notifier)
    {
        if (_allegedParent == null)
        {
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"Filter {_child} is not cloned from a Catalogue filter so does not need checking for synchronicity",
                    CheckResult.Success));

            return;
        }

        //we were cloned from a filter in the Catalogue
        var exist = _catalogueDatabaseRepository.StillExists<ExtractionFilter>((int)_allegedParent);

        //tell them if it has been nuked
        notifier.OnCheckPerformed(
            new CheckEventArgs(
                $"Catalogue reports that the original filter we were cloned from {(exist ? " still exists " : " no longer exists")}",
                exist ? CheckResult.Success : CheckResult.Warning));

        //it hasn't been nuked
        if (exist)
        {
            //get it
            var parent = _catalogueDatabaseRepository.GetObjectByID<ExtractionFilter>((int)_allegedParent);

            if (string.IsNullOrWhiteSpace(parent.WhereSQL) || string.IsNullOrWhiteSpace(_child.WhereSQL))
                return;

            //see if someone has been monkeying with the parent (or the child) in which case warn them about the disparity
            if (parent.WhereSQL.Equals(_child.WhereSQL))
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Filter {_child} has the same WhereSQL as parent",
                    CheckResult.Success));
            else
                try
                {
                    throw new ExpectedIdenticalStringsException("Expected WHERE SQL to be identical", parent.WhereSQL,
                        _child.WhereSQL);
                }
                catch (ExpectedIdenticalStringsException ex)
                {
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            $"{_child.GetType().Name} called '{_child}' (ID={_child.ID}) WhereSQL does not match the parent it was originally cloned from (ExtractionFilter ID={_allegedParent}).  You might have made a deliberate change in your copy or it might mean that someone has corrected the parent since you first cloned it",
                            CheckResult.Warning, ex));
                }
        }
    }
}