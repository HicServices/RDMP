// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation.Checks;

/// <summary>
///     Checks an IPipeline (persisted data flow pipeline configuration) to see if all its components are constructable
///     (using MEFChecker)
/// </summary>
public class PipelineChecker : ICheckable
{
    private readonly IPipeline _pipeline;

    /// <summary>
    ///     Sets up the checker to check the supplied pipeline
    /// </summary>
    /// <param name="pipeline"></param>
    public PipelineChecker(IPipeline pipeline)
    {
        _pipeline = pipeline;
    }

    /// <summary>
    ///     Checks that all the components defined in the pipeline are found using a MEFChecker.  This will also handle classes
    ///     changing namespaces by updating
    ///     class name reference.
    /// </summary>
    /// <param name="notifier"></param>
    public void Check(ICheckNotifier notifier)
    {
        foreach (var component in _pipeline.PipelineComponents)
        {
            var copy = component;
            var mefChecker = new MEFChecker(component.Class, delegate(string s)
            {
                copy.Class = s;
                copy.SaveToDatabase();
            });
            mefChecker.Check(notifier);
        }
    }
}