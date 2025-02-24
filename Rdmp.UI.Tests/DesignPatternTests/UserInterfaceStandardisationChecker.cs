// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Rdmp.Core.Providers;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Providers.Nodes.PipelineNodes;
using Rdmp.Core.Providers.Nodes.UsedByNodes;
using Rdmp.Core.Repositories;
using Rdmp.UI.CatalogueSummary.DataQualityReporting;
using Rdmp.UI.CatalogueSummary.LoadEvents;
using Rdmp.UI.CommandExecution.Proposals;
using Rdmp.UI.DashboardTabs;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Menus;
using Rdmp.UI.Overview;
using Rdmp.UI.PieCharts;
using Rdmp.UI.Raceway;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ResearchDataManagementPlatform;

namespace Rdmp.UI.Tests.DesignPatternTests;

public partial class UserInterfaceStandardisationChecker
{
    private List<string> _csFilesList;
    private List<string> problems = new();

    private Type[] excusedNodeClasses =
    {
        //it's a singleton because you can only have one decryption certificate for an RDMP as opposed to other SingletonNode classes that represent collections e.g. AllTableInfos is the only collection of TableInfos but it's a collection
        typeof(DecryptionPrivateKeyNode),
        typeof(ArbitraryFolderNode),

        //excused because although singletons they have dynamic names / they are basically a collection
        typeof(OtherPipelinesNode),
        typeof(StandardPipelineUseCaseNode),

        //the base class
        typeof(Node)
    };


    /// <summary>
    /// UI classes that are allowed not to end with the suffix UI
    /// </summary>
    private Type[] excusedUIClasses =
    {
        typeof(RDMPUserControl),
        typeof(RDMPForm),
        typeof(RDMPSingleDatabaseObjectControl<>),
        typeof(DashboardableControlHostPanel),
        typeof(TimePeriodicityChart),
        typeof(LoadEventsTreeView),
        typeof(GoodBadCataloguePieChart),
        typeof(DatasetRaceway),
        typeof(ResolveFatalErrors),
        typeof(DataLoadsGraph),
        typeof(RDMPMainForm)
    };

    public void FindProblems(List<string> csFilesList)
    {
        _csFilesList = csFilesList;

        //All node classes should have equality compare members so that tree expansion works properly
        foreach (var nodeClass in MEF.GetAllTypes()
                     .Where(t => typeof(Node).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface))
        {
            if (nodeClass.Namespace == null || nodeClass.Namespace.StartsWith("System"))
                continue;

            //class is excused
            if (excusedNodeClasses.Contains(nodeClass))
                continue;

            //it's something like ProposeExecutionWhenTargetIsIDirectoryNode.cs i.e. it's not a Node!
            if (typeof(ICommandExecutionProposal).IsAssignableFrom(nodeClass))
                continue;

            //if it's an ObjectUsedByOtherObjectNode then it will already have GetHashCode implemented
            if (typeof(IObjectUsedByOtherObjectNode).IsAssignableFrom(nodeClass))
                continue;

            if (typeof(ExtractionArbitraryFolderNode).IsAssignableFrom(nodeClass))
                continue;

            //these are all supported at base class level
            if (typeof(SingletonNode).IsAssignableFrom(nodeClass))
            {
                if (!nodeClass.Name.StartsWith("All"))
                    problems.Add($"Class '{nodeClass.Name}' is a SingletonNode but its name doesn't start with All");

                continue;
            }

            ConfirmFileHasText(nodeClass, "public override int GetHashCode()");
        }

        //All Menus should correspond to a data class
        foreach (var menuClass in MEF.GetAllTypes().Where(t =>
                     typeof(RDMPContextMenuStrip).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface))
        {
            //the basic class from which all are inherited or a menu for FolderNode<X>
            if (menuClass == typeof(RDMPContextMenuStrip) || menuClass.Name.EndsWith("FolderMenu"))
                continue;

            //We are looking at something like AutomationServerSlotsMenu
            if (!menuClass.Name.EndsWith("Menu"))
            {
                problems.Add($"Class '{menuClass}' is a RDMPContextMenuStrip but its name doesn't end with Menu");
                continue;
            }

            foreach (var c in menuClass.GetConstructors())
                if (c.GetParameters().Length != 2)
                    problems.Add(
                        $"Constructor of class '{menuClass}' which is an RDMPContextMenuStrip contained {c.GetParameters().Length} constructor arguments.  These menus are driven by reflection (See RDMPCollectionCommonFunctionality.GetMenuWithCompatibleConstructorIfExists )");


            var toLookFor = menuClass.Name[..^"Menu".Length];
            var expectedClassName = GetExpectedClassOrInterface(toLookFor);

            if (expectedClassName == null)
            {
                problems.Add(
                    $"Found menu called '{menuClass.Name}' but couldn't find a corresponding data class called '{toLookFor}.cs'");
                continue;
            }

            ConfirmFileHasText(menuClass, "AddCommonMenuItems()", false);

            //expect something like this
            //public AutomationServerSlotsMenu(IActivateItems activator, AllAutomationServerSlotsNode databaseEntity)
            var expectedConstructorSignature = $"{menuClass.Name}(RDMPContextMenuStripArgs args,{expectedClassName}";
            ConfirmFileHasText(menuClass, expectedConstructorSignature);

            var fields = menuClass.GetFields(
                BindingFlags.NonPublic |
                BindingFlags.Instance);

            //find private fields declared at the object level (i.e. not in base class that are of type IActivateItem)
            var activatorField =
                fields.FirstOrDefault(f => f.DeclaringType == menuClass && f.FieldType == typeof(IActivateItems));
            if (activatorField != null)
                problems.Add(
                    $"Menu '{menuClass}' contains a private field called '{activatorField.Name}'.  You should instead use base class protected field RDMPContextMenuStrip._activator");
        }

        //Drag and drop / Activation - Execution Proposal system
        foreach (var proposalClass in MEF.GetAllTypes().Where(t =>
                     typeof(ICommandExecutionProposal).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface))
        {
            if (proposalClass.Namespace.Contains("Rdmp.UI.Tests.DesignPatternTests"))
                continue;

            //We are looking at something like AutomationServerSlotsMenu
            if (!proposalClass.Name.StartsWith("ProposeExecutionWhenTargetIs"))
            {
                problems.Add(
                    $"Class '{proposalClass}' is a ICommandExecutionProposal but its name doesn't start with ProposeExecutionWhenTargetIs");
                continue;
            }

            var toLookFor = proposalClass.Name["ProposeExecutionWhenTargetIs".Length..];
            var expectedClassName = GetExpectedClassOrInterface(toLookFor);

            if (expectedClassName == null)
                problems.Add(
                    $"Found proposal called '{proposalClass}' but couldn't find a corresponding data class called '{toLookFor}.cs'");
        }

        //Make sure all user interface classes have the suffix UI
        foreach (var uiType in MEF.GetAllTypes()
                     .Where(static t => typeof(RDMPUserControl).IsAssignableFrom(t) ||
                                        (typeof(RDMPForm).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface))
                     .Where(static uiType =>
                         !uiType.Name.EndsWith("UI", StringComparison.Ordinal) &&
                         !uiType.Name.EndsWith("_Design", StringComparison.Ordinal))
                     .Where(uiType => !excusedUIClasses.Contains(uiType))
                     .Where(static uiType => !ScreenN().IsMatch(uiType.Name) || !uiType.IsNotPublic))
            problems.Add($"Class {uiType.Name} does not end with UI");


        foreach (var problem in problems)
            Console.WriteLine($"FATAL ERROR PROBLEM:{problem}");

        Assert.That(problems, Is.Empty);
    }

    private string GetExpectedClassOrInterface(string expectedClassName)
    {
        //found it?
        if (_csFilesList.Any(f =>
                Path.GetFileName(f).Equals($"{expectedClassName}.cs", StringComparison.InvariantCultureIgnoreCase)))
            return expectedClassName;

        //expected Filter but found IFilter - acceptable
        return _csFilesList.Any(f =>
            Path.GetFileName(f).Equals($"I{expectedClassName}.cs", StringComparison.InvariantCultureIgnoreCase))
            ? $"I{expectedClassName}"
            : null;
    }

    private void ConfirmFileHasText(Type type, string expectedString, bool mustHaveText = true)
    {
        var file = _csFilesList.SingleOrDefault(f => Path.GetFileName(f).Equals($"{type.Name}.cs"));

        //probably not our class
        if (file == null)
            return;

        var hasText = Regex.Replace(File.ReadAllText(file), "[ \r\n\t]+", "")
            .Contains(expectedString.Replace(" ", ""), StringComparison.OrdinalIgnoreCase);

        if (mustHaveText)
        {
            if (!hasText)
                problems.Add($"File '{file}' did not contain expected text '{expectedString}'");
        }
        else
        {
            if (hasText)
                problems.Add($"File '{file}' contains unexpected text '{expectedString}'");
        }
    }

    [GeneratedRegex("Screen\\d", RegexOptions.CultureInvariant)]
    private static partial Regex ScreenN();
}