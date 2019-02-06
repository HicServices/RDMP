// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ReusableLibraryCode.Performance;
using ReusableUIComponents.Dependencies;

namespace ReusableUIComponents.Performance.PerformanceStackPath
{
    /// <summary>
    /// Displays the same information as PerformanceCounterResultsUI but in a call hierarchy showing which stack frames are issuing database queries and the flow of execution between
    /// them.  Note that because database queries are logged in realtime (See PerformanceCounterUI) you will always get a node called 'Origin' from which all root stack frames are attached.
    /// 
    /// <para>This means if you turn on performance logging and right click a node then delete something else you should see 2 (or more) branches from Origin that relate to those actions (though
    /// you might find common trunk frames e.g. Program.main etc.</para>
    /// </summary>
    public partial class PerformanceStackPathViewerUI : UserControl
    {
        DependencyGraph g; 
        StackFrame origin;

        public PerformanceStackPathViewerUI(ComprehensiveQueryPerformanceCounter performanceCounter, int worstOffenderCount, bool collapseToMethod)
        {
            InitializeComponent();

            origin = new StackFrame("Origin");

            List<StackFrame> framesSeen = new List<StackFrame>();

            foreach (string stackTraces in performanceCounter.DictionaryOfQueries.Firsts)
            {
                var lines = stackTraces.Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).Reverse();
                var performed = performanceCounter.DictionaryOfQueries.GetByFirst(stackTraces);

                StackFrame previous = origin;

                //for each line in the stack trace
                foreach (string line in lines)
                {
                    string mutilatedLine = line;

                    if(collapseToMethod)
                    {
                        string filenameMatch;
                        int lineNumberMatch;
                        string methodMatch;

                        if(!ExceptionViewerStackTraceWithHyperlinks.MatchStackLine(line, out filenameMatch,out lineNumberMatch, out methodMatch))
                            continue;//it's a dead line

                        mutilatedLine = methodMatch;
                    }
                    
                    //find a matching node that already exists
                    var matchingFrame = framesSeen.SingleOrDefault(f => f.Frame.Equals(mutilatedLine));
                    
                    //or identify it as a unique new one
                    if(matchingFrame == null)
                    {
                        matchingFrame = new StackFrame(mutilatedLine);
                        framesSeen.Add(matchingFrame);
                    }

                    //record the query statistics (number of queries sent)
                    matchingFrame.Document(performed);

                    //now audcit the previous node calls it
                    previous.DocumentThatItCalls(matchingFrame);
                    previous = matchingFrame;
                }
            }

            g = new DependencyGraph(new []{typeof(StackFrame)}, new StackFrameVisualiser(worstOffenderCount));
            g.Dock = DockStyle.Fill;

            Controls.Add(g);
        }

        private void PerformanceStackPathViewerUI_Load(object sender, EventArgs e)
        {
            g.GraphDependenciesOf(origin);

        }
    }
}
