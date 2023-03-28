// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.UI.Collections;
using Rdmp.UI.Performance.StackTraceProcessing;
using ReusableLibraryCode.Performance;

namespace Rdmp.UI.Performance;

/// <summary>
/// Displays detailed breakdown of database queries sent by the RDMP during Performance Logging (See PerformanceCounterUI).  The colour of the row indicates the number of times a database
/// query was sent from that point in the call stack.  Note that this is the number of calls not the time taken to execute the call so you could see poor performance in UI interaction and
/// see lots of red calls but the actual slow query might only be called once. 
/// 
/// </summary>
public partial class PerformanceCounterResultsUI : UserControl
{
    private static Color Heavy = Color.Red;
    private static Color Light = Color.LawnGreen;

    public PerformanceCounterResultsUI()
    {
        InitializeComponent();

        tlvLocations.CanExpandGetter += CanExpandGetter;
        tlvLocations.ChildrenGetter += ChildrenGetter;
        tlvLocations.RowFormatter += RowFormatter;

        RDMPCollectionCommonFunctionality.SetupColumnTracking(tlvLocations, olvQueryCount, new Guid("576c87c7-a324-45bb-a4bc-4757044f7c43"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(tlvLocations, olvQueryCount, new Guid("905e5565-0428-4734-8582-9734546d55a3"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(tlvLocations, olvStackFrame, new Guid("865f84a1-f0c4-48d9-8983-10d35cf4a513"));
    }

    private void RowFormatter(OLVListItem olvItem)
    {
        var o = (StackFramesTree)olvItem.RowObject;
            
        //bold the endpoints - where there are no children or they are all in database assemblies
        if (!o.Children.Any() ||o.Children.Values.All(c => c.IsInDatabaseAccessAssembly))
            olvItem.Font = new Font(olvItem.Font,FontStyle.Bold);

        var fraction = (double) o.QueryCount/_worstOffenderCount;
        olvItem.BackColor = GetHeat(fraction);

    }

    private IEnumerable ChildrenGetter(object model)
    {
        var treeNode = (StackFramesTree)model;

        if (!treeNode.Children.Any())
            return null;

        return treeNode.Children.Values.Where(c=>!c.IsInDatabaseAccessAssembly);
    }

    private bool CanExpandGetter(object model)
    {
        var treeNode = (StackFramesTree)model;

        if (!treeNode.Children.Any())
            return false;

        return treeNode.Children.Values.Any(c=>!c.IsInDatabaseAccessAssembly);
    }

    List<StackFramesTree> Roots;

    bool collapseToMethod = false;
    private ComprehensiveQueryPerformanceCounter _performanceCounter;
        
    private int _worstOffenderCount;

    public static Color GetHeat(double fraction)
    {
        var r = Light.R + ((Heavy.R - Light.R) * fraction);
        var g = Light.G + ((Heavy.G - Light.G) * fraction);
        var b = Light.B + ((Heavy.B - Light.B) * fraction);

        return Color.FromArgb((int)r, (int)g, (int)b);
    }

    public void LoadState(ComprehensiveQueryPerformanceCounter performanceCounter)
    {
        _performanceCounter = performanceCounter;

        Roots = new List<StackFramesTree>();

        _worstOffenderCount = performanceCounter.DictionaryOfQueries.Values.Sum(k => k.TimesSeen);
        Regex isSystemCall = new Regex(@"^\s*(at)?\s*System.Windows.Forms");

        //for each documented query point (which has a stack trace)
        foreach (string stackTrace in performanceCounter.DictionaryOfQueries.Keys)
        {
            //get the query
            var query = performanceCounter.DictionaryOfQueries[stackTrace];

            //get the stack trace split by line reversed so the root is at the top
            var lines = stackTrace.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).Reverse().ToArray();
                   
            lines = lines.Where(l=>!isSystemCall.IsMatch(l)).ToArray();
                
            if(lines.Length == 0)
                continue;
                
            if(collapseToMethod)
            {
                    
                List<string> uniqueMethodLines = new List<string>();

                string lastMethodName = StackFramesTree.GetMethodName(lines[0]);
                uniqueMethodLines.Add(lines[0]);
                    
                for (int i = 1; i < lines.Length; i++)
                {
                    string currentMethodName = StackFramesTree.GetMethodName(lines[i]);
                        
                    //if there is no method name or it is not new
                    if(currentMethodName == null || lastMethodName.Equals(currentMethodName))
                        continue;

                    lastMethodName = currentMethodName;
                    uniqueMethodLines.Add(lines[i]);
                }


                lines = uniqueMethodLines.ToArray();
            }

            StackFramesTree currentRoot = null;

            //if we have seen the root before?
            currentRoot = Roots.SingleOrDefault(n => n.CurrentFrame.Equals(lines[0]));

            //it's novel
            if (currentRoot == null)
            {
                //add a new root level entrypoint
                currentRoot = new StackFramesTree(lines, query,false);
                Roots.Add(currentRoot);
            }
            else
                currentRoot.AddSubframes(lines, query);
        }
            
        //if there is one root node
        if(Roots.Count == 1)
        {
            //find the first point at which it splits
            var firstBranch = Roots.Single();

            //still hasn't split
            while (firstBranch.Children.Count == 1)
                firstBranch = firstBranch.Children.Values.Single();

            //if it did split
            if(firstBranch.Children.Count > 1)
                Roots = new List<StackFramesTree>(new []{firstBranch});
        }

            
        tlvLocations.ClearObjects();
        tlvLocations.AddObjects(Roots);
        tlvLocations.ExpandAll();
    }

    private void tlvLocations_ItemActivate(object sender, EventArgs e)
    {
        var model = tlvLocations.SelectedObject as StackFramesTree;

        if (model != null)
        {
            if(model.HasSourceCode)
            {
                var dialog = new Rdmp.UI.SimpleDialogs.ViewSourceCodeDialog(model.Filename,model.LineNumber, Color.GreenYellow);
                dialog.Show();
            }
        }
            
    }

    private void tbFilter_TextChanged(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(tbFilter.Text))
        {
            tlvLocations.UseFiltering = false;
            tlvLocations.ModelFilter = null;
        }
        else
        {
            tlvLocations.ModelFilter = new TextMatchFilter(tlvLocations,tbFilter.Text);
            tlvLocations.UseFiltering = true;
        }
    }
}