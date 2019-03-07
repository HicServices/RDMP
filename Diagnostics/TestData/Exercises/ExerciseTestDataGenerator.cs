// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using ReusableLibraryCode.Progress;

namespace Diagnostics.TestData.Exercises
{
    [InheritedExport(typeof(IExerciseTestDataGenerator))]
    public abstract class ExerciseTestDataGenerator : IExerciseTestDataGenerator
    {
        public void GenerateTestDataFile(IExerciseTestIdentifiers cohort, FileInfo target, int numberOfRecords,
            IDataLoadEventListener listener)
        {
            Random r = new Random();
            int totalPeople = cohort.People.Length;

            int linesWritten;
            StreamWriter sw = new StreamWriter(target.FullName);

            WriteHeaders(sw);

            Stopwatch stopwatch = new Stopwatch();
            string task = "Populate " + target.Name;
            stopwatch.Start();

            for (linesWritten = 0; linesWritten < numberOfRecords; linesWritten++)
            {
                sw.WriteLine(string.Join(",", GenerateTestDataRow(cohort.People[r.Next(totalPeople)])));

                if (linesWritten % 1000 == 0)
                {
                    listener.OnProgress(this, new ProgressEventArgs(task, new ProgressMeasurement(linesWritten + 1, ProgressType.Records), stopwatch.Elapsed));
                    sw.Flush();//flush every 1000
                }
            }

            //tell them about the last line written
            listener.OnProgress(this, new ProgressEventArgs(task, new ProgressMeasurement(linesWritten, ProgressType.Records), stopwatch.Elapsed));

            stopwatch.Stop();
            sw.Close();
        }

        public abstract string GetName();

        protected abstract object[] GenerateTestDataRow(TestPerson p);
        protected abstract void WriteHeaders(StreamWriter sw);


        public static void WriteLookups(DirectoryInfo dir)
        {
            File.WriteAllText(Path.Combine(dir.FullName, "z_Healthboards.csv"),
@"A,Ayrshire and Arran
B,Borders
C,Argyle and Clyde
D,State Hospital
E,England
F,Fife
G,Greater Glasgow
H,Highland
I,Inverness
J,Junderland
K,Krief
L,Lanarkshire
M,Metropolitan Area
N,Grampian
O,Orkney
P,Pitlochry
Q,Queensferry
R,Retired
S,Lothian
T,Tayside
U,Unknown
V,Forth Valley
W,Western Isles
X,Common Service Agency
Y,Dumfries and Galloway
Z,Shetland");
        }

    }
}