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
using Rdmp.Core.Providers;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Repositories;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.Menus;
using Rdmp.UI.Tests.DesignPatternTests.ClassFileEvaluation;
using ReusableLibraryCode.Checks;

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

        private class _a93fd8b3d1fb4ad8975ef8cf9c384236
        {
            #region a93fd8b3d1fb4ad8975ef8cf9c384236
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

                protected bool Equals(FrozenExtractionConfigurationsNode other)
                {
                    return Equals(Project, other.Project);
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj)) return false;
                    if (ReferenceEquals(this, obj)) return true;
                    if (obj.GetType() != this.GetType()) return false;
                    return Equals((FrozenExtractionConfigurationsNode) obj);
                }

                public override int GetHashCode()
                {
                    return (Project != null ? Project.GetHashCode() : 0);
                }
            }
            #endregion


            private class _c9aeab3ddaf643e5967c3e2352c388f0 : DataExportChildProvider
            {
                #region c9aeab3ddaf643e5967c3e2352c388f0
                private void AddChildren(ExtractionConfigurationsNode extractionConfigurationsNode, DescendancyList descendancy)
                {
                    HashSet<object> children = new HashSet<object>();

                    var frozenConfigurationsNode = new FrozenExtractionConfigurationsNode(extractionConfigurationsNode.Project);
                    children.Add(frozenConfigurationsNode);

                    var configs = ExtractionConfigurations.Where(c => c.Project_ID == extractionConfigurationsNode.Project.ID).ToArray();
                    foreach (ExtractionConfiguration config in configs)
                    {
                        AddChildren(config, descendancy.Add(config));
                        children.Add(config);
                    }

                    AddToDictionaries(children, descendancy);
                }
                #endregion

                private void AddChildren(ExtractionConfiguration extractionConfigurationsNode, DescendancyList descendancy)
                {
                    throw new NotImplementedException();
                }
                public _c9aeab3ddaf643e5967c3e2352c388f0(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IChildProvider[] pluginChildProviders, ICheckNotifier errorsCheckNotifier) : base(repositoryLocator, pluginChildProviders, errorsCheckNotifier)
                {
                }
            }


            private class _0bac9aa7f8874a25bc1fe1361b91f6e5 : DataExportChildProvider
            {
                #region 0bac9aa7f8874a25bc1fe1361b91f6e5
                private void AddChildren(ExtractionConfigurationsNode extractionConfigurationsNode, DescendancyList descendancy)
                {
                    HashSet<object> children = new HashSet<object>();

                    //Create a frozen extraction configurations folder as a subfolder of each ExtractionConfigurationsNode
                    var frozenConfigurationsNode = new FrozenExtractionConfigurationsNode(extractionConfigurationsNode.Project);

                    //Make the frozen folder appear under the extractionConfigurationsNode
                    children.Add(frozenConfigurationsNode);

                    //Add children to the frozen folder
                    AddChildren(frozenConfigurationsNode,descendancy.Add(frozenConfigurationsNode));

                    //Add ExtractionConfigurations which are not released (frozen)
                    var configs = ExtractionConfigurations.Where(c => c.Project_ID == extractionConfigurationsNode.Project.ID).ToArray();
                    foreach (ExtractionConfiguration config in configs.Where(c=>!c.IsReleased))
                    {
                        AddChildren(config, descendancy.Add(config));
                        children.Add(config);
                    }

                    AddToDictionaries(children, descendancy);
                }

                private void AddChildren(FrozenExtractionConfigurationsNode frozenExtractionConfigurationsNode, DescendancyList descendancy)
                {
                    HashSet<object> children = new HashSet<object>();

                    //Add ExtractionConfigurations which are not released (frozen)
                    var configs = ExtractionConfigurations.Where(c => c.Project_ID == frozenExtractionConfigurationsNode.Project.ID).ToArray();
                    foreach (ExtractionConfiguration config in configs.Where(c => c.IsReleased))
                    {
                        AddChildren(config, descendancy.Add(config));
                        children.Add(config);
                    }

                    AddToDictionaries(children,descendancy);
                }
                #endregion
                
                private void AddChildren(ExtractionConfiguration frozenExtractionConfigurationsNode, DescendancyList descendancy)
                {
                    throw new NotImplementedException();
                }

                public _0bac9aa7f8874a25bc1fe1361b91f6e5(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IChildProvider[] pluginChildProviders, ICheckNotifier errorsCheckNotifier) : base(repositoryLocator, pluginChildProviders, errorsCheckNotifier)
                {
                }
            }

            private class _f243e95a6dc94b3486f44b8f0bb0ed7d
            {
                #region f243e95a6dc94b3486f44b8f0bb0ed7d
                class AllServersNodeMenu : RDMPContextMenuStrip
                {
                    public AllServersNodeMenu(RDMPContextMenuStripArgs args, AllServersNode o) : base(args, o)
                    {
                        Add(new ExecuteCommandCreateNewEmptyCatalogue(args.ItemActivator));
                    }
                }
                #endregion
            }

            
        }
    }
}
