// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Rdmp.Core.DataExport.Data;
using Rdmp.UI.Tests.DesignPatternTests.ClassFileEvaluation;

namespace Rdmp.UI.Tests.DesignPatternTests
{
    /// <summary>
    /// This class exists to ensure that code blocks in the markdown documentation compile (at least!).  The guid matches the guid in the markdown file.  The method
    /// <see cref="DocumentationCrossExaminationTest.EnsureCodeBlocksCompile"/> checks that the code in the markdown matches the code here within the same guid region.
    /// </summary>
    class MarkdownCodeBlockTests
    {
        #region df7d2bb4cd6145719f933f6f15218b1a
        class FrozenExtractionConfigurationsNode
        {
            public Project Project { get; set; }

            public FrozenExtractionConfigurationsNode(Project project)
            {
                Project = project;
            }

            public override string ToString()
            {
                return "Frozen Extraction Configurations";
            }
        }
        #endregion
    }
}
