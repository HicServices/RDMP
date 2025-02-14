// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Providers;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.UI.AggregationUIs.Advanced;
using Rdmp.UI.CommandExecution;
using Rdmp.UI.CommandExecution.Proposals;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Menus;
using Rdmp.UI.Tests.DesignPatternTests.ClassFileEvaluation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.Tests.DesignPatternTests;

/// <summary>
/// This class exists to ensure that code blocks in the markdown documentation compile (at least!).  The guid matches the guid in the markdown file.  The method
/// <see cref="DocumentationCrossExaminationTest.EnsureCodeBlocksCompile"/> checks that the code in the markdown matches the code here within the same guid region.
/// </summary>
internal class MarkdownCodeBlockTests
{
    #region df7d2bb4cd6145719f933f6f15218b1a

    private class FrozenExtractionConfigurationsNode
    {
        public Project Project { get; set; }

        public FrozenExtractionConfigurationsNode(Project project)
        {
            Project = project;
        }

        public override string ToString() => "Frozen Extraction Configurations";
    }

    #endregion

    private class _a93fd8b3d1fb4ad8975ef8cf9c384236
    {
        #region a93fd8b3d1fb4ad8975ef8cf9c384236

        private class FrozenExtractionConfigurationsNode
        {
            public Project Project { get; set; }

            public FrozenExtractionConfigurationsNode(Project project)
            {
                Project = project;
            }

            public override string ToString() => "Frozen Extraction Configurations";

            protected bool Equals(FrozenExtractionConfigurationsNode other) => Equals(Project, other.Project);

            public override bool Equals(object obj)
            {
                if (obj is null) return false;
		        if (ReferenceEquals(this, obj)) return true;
		        return obj.GetType() == GetType() && Equals((FrozenExtractionConfigurationsNode)obj);
            }

            public override int GetHashCode() => Project?.GetHashCode() ?? 0;
        }

        #endregion


        private class _c9aeab3ddaf643e5967c3e2352c388f0 : DataExportChildProvider
        {
            #region c9aeab3ddaf643e5967c3e2352c388f0

            private void AddChildren(ExtractionConfigurationsNode extractionConfigurationsNode,
                DescendancyList descendancy)
            {
                var children = new HashSet<object>();

                var frozenConfigurationsNode =
                    new FrozenExtractionConfigurationsNode(extractionConfigurationsNode.Project);
                children.Add(frozenConfigurationsNode);

                var configs = ExtractionConfigurations
                    .Where(c => c.Project_ID == extractionConfigurationsNode.Project.ID).ToArray();
                foreach (var config in configs)
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

            public _c9aeab3ddaf643e5967c3e2352c388f0(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
                IChildProvider[] pluginChildProviders, ICheckNotifier errorsCheckNotifier) : base(repositoryLocator,
                pluginChildProviders, errorsCheckNotifier, null)
            {
            }
        }


        private class _0bac9aa7f8874a25bc1fe1361b91f6e5 : DataExportChildProvider
        {
            #region 0bac9aa7f8874a25bc1fe1361b91f6e5

            private void AddChildren(ExtractionConfigurationsNode extractionConfigurationsNode,
                DescendancyList descendancy)
            {
                var children = new HashSet<object>();

                //Create a frozen extraction configurations folder as a subfolder of each ExtractionConfigurationsNode
                var frozenConfigurationsNode =
                    new FrozenExtractionConfigurationsNode(extractionConfigurationsNode.Project);

                //Make the frozen folder appear under the extractionConfigurationsNode
                children.Add(frozenConfigurationsNode);

                //Add children to the frozen folder
                AddChildren(frozenConfigurationsNode, descendancy.Add(frozenConfigurationsNode));

                //Add ExtractionConfigurations which are not released (frozen)
                var configs = ExtractionConfigurations
                    .Where(c => c.Project_ID == extractionConfigurationsNode.Project.ID).ToArray();
                foreach (var config in configs.Where(c => !c.IsReleased))
                {
                    AddChildren(config, descendancy.Add(config));
                    children.Add(config);
                }

                AddToDictionaries(children, descendancy);
            }

            private void AddChildren(FrozenExtractionConfigurationsNode frozenExtractionConfigurationsNode,
                DescendancyList descendancy)
            {
                var children = new HashSet<object>();

                //Add ExtractionConfigurations which are not released (frozen)
                var configs = ExtractionConfigurations
                    .Where(c => c.Project_ID == frozenExtractionConfigurationsNode.Project.ID).ToArray();
                foreach (var config in configs.Where(c => c.IsReleased))
                {
                    AddChildren(config, descendancy.Add(config));
                    children.Add(config);
                }

                AddToDictionaries(children, descendancy);
            }

            #endregion

            private void AddChildren(ExtractionConfiguration frozenExtractionConfigurationsNode,
                DescendancyList descendancy)
            {
                throw new NotImplementedException();
            }

            public _0bac9aa7f8874a25bc1fe1361b91f6e5(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
                IChildProvider[] pluginChildProviders, ICheckNotifier errorsCheckNotifier) : base(repositoryLocator,
                pluginChildProviders, errorsCheckNotifier, null)
            {
            }
        }

        private class _f243e95a6dc94b3486f44b8f0bb0ed7d
        {
            #region f243e95a6dc94b3486f44b8f0bb0ed7d

            private class AllServersNodeMenu : RDMPContextMenuStrip
            {
                public AllServersNodeMenu(RDMPContextMenuStripArgs args, AllServersNode o) : base(args, o)
                {
                    Add(new ExecuteCommandCreateNewEmptyCatalogue(args.ItemActivator));
                }
            }

            #endregion
        }

        private class _cae13dde1de14f5cac984330a222c311
        {
            #region cae13dde1de14f5cac984330a222c311

            private class ProposeExecutionWhenTargetIsPipeline : RDMPCommandExecutionProposal<Pipeline>
            {
                public ProposeExecutionWhenTargetIsPipeline(IActivateItems itemActivator) : base(itemActivator)
                {
                }

                public override bool CanActivate(Pipeline target) => true;

                public override void Activate(Pipeline target)
                {
                    MessageBox.Show("Double clicked");
                }

                public override ICommandExecution ProposeExecution(ICombineToMakeCommand cmd, Pipeline target,
                    InsertOption insertOption = InsertOption.Default) => null;
            }

            #endregion
        }

        private class _d5ff7bebc57942df8c6c57a316bf72c6 : RDMPCommandExecutionProposal<AggregateConfiguration>
        {
            public override bool CanActivate(AggregateConfiguration target) => throw new NotImplementedException();

            public override ICommandExecution ProposeExecution(ICombineToMakeCommand cmd, AggregateConfiguration target,
                InsertOption insertOption = InsertOption.Default) =>
                throw new NotImplementedException();

            public _d5ff7bebc57942df8c6c57a316bf72c6(IActivateItems itemActivator) : base(itemActivator)
            {
            }

            #region d5ff7bebc57942df8c6c57a316bf72c6

            public override void Activate(AggregateConfiguration target)
            {
                ItemActivator.Activate<AggregateEditorUI, AggregateConfiguration>(target);
            }

            #endregion


            #region 56df0867990f4b0397e51a6a49f7bdd0

            [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ANOTableUI_Design, UserControl>))]
            public abstract class ANOTableUI_Design : RDMPSingleDatabaseObjectControl<ANOTable>;

            #endregion
        }

        private class _59f55fa3ef50404291c7ae3996772635 : RDMPCommandExecutionProposal<Pipeline>
        {
            public override bool CanActivate(Pipeline target) => throw new NotImplementedException();

            public override void Activate(Pipeline target)
            {
                throw new NotImplementedException();
            }

            #region 59f55fa3ef50404291c7ae3996772635

            public override ICommandExecution ProposeExecution(ICombineToMakeCommand cmd, Pipeline target,
                InsertOption insertOption = InsertOption.Default) =>
                cmd is CatalogueCombineable sourceCatalogueCombineable
                    ? new ExecuteCommandDelete(ItemActivator, sourceCatalogueCombineable.Catalogue)
                    : (ICommandExecution)null;

            #endregion

            public _59f55fa3ef50404291c7ae3996772635(IActivateItems itemActivator) : base(itemActivator)
            {
            }
        }

        private class _bbee6cb18ebd4e35a19f5fa521063648
        {
            #region bbee6cb18ebd4e35a19f5fa521063648

            public class PipelineCombineable : ICombineToMakeCommand
            {
                public Pipeline Pipeline { get; private set; }
                public bool IsEmpty { get; private set; }

                public PipelineCombineable(Pipeline pipeline)
                {
                    Pipeline = pipeline;
                    IsEmpty = Pipeline.PipelineComponents.Count == 0;
                }

                public string GetSqlString() => "";
            }

            #endregion
        }
    }
}