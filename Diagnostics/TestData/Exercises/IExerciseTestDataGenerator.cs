// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using ReusableLibraryCode.Progress;

namespace Diagnostics.TestData.Exercises
{
    public interface IExerciseTestDataGenerator
    {
        /// <summary>
        /// Create the dataset in the given file location using person identifiers in the <paramref name="cohort"/>
        /// </summary>
        /// <param name="cohort">All people in the test data cohort, allows linkage between different randomly generated test datasets</param>
        /// <param name="target">The file that will be created</param>
        /// <param name="numberOfRecords">The number of fake data records that should appear in the file created</param>
        /// <param name="listener">Notify events like success/fail etc</param>
        void GenerateTestDataFile(IExerciseTestIdentifiers cohort, FileInfo target, int numberOfRecords, IDataLoadEventListener listener);
        
        /// <summary>
        /// Name of the dataset e.g. "prescribing" determines how it is announced to user and the default file names generated e.g. "prescribing.csv"
        /// </summary>
        /// <returns></returns>
        string GetName();
    }

}
