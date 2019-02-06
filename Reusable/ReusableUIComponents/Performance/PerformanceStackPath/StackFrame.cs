// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ReusableLibraryCode;
using ReusableLibraryCode.Performance;

namespace ReusableUIComponents.Performance.PerformanceStackPath
{
    public class StackFrame :IHasDependencies
    {
        public string Frame { get; set; }

        public HashSet<StackFrame> CalledFrames { get; private set; }
        public int TimesCalled;

        public StackFrame(string frame)
        {
            Frame = frame;
            CalledFrames = new HashSet<StackFrame>();
        }

        Regex rMethodNameIfAny = new Regex(@"([^\.\>]*)\(");
        public string GetMethodName()
        {
            var m=  rMethodNameIfAny.Match(Frame);
            if (m.Success)
                return m.Groups[1].Value;

            return Frame;
        }

        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return new IHasDependencies[0];
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            return CalledFrames.ToArray();
        }

        private HashSet<QueryPerformed> AlreadyDocumented = new HashSet<QueryPerformed>();
        public void Document(QueryPerformed performed)
        {
            //indicates a recursive call, don't double document
            if (AlreadyDocumented.Contains(performed))
                return;

            TimesCalled += performed.TimesSeen;
            AlreadyDocumented.Add(performed);
        }

        public void DocumentThatItCalls(StackFrame matchingFrame)
        {
            //if it is novel
            if (!CalledFrames.Any(f => f.Frame.Equals(matchingFrame.Frame)))
                CalledFrames.Add(matchingFrame);//document that we call it
        }
    }
}
