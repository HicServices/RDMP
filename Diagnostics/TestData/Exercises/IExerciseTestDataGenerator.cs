using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ReusableLibraryCode.Progress;

namespace Diagnostics.TestData.Exercises
{
    public interface IExerciseTestDataGenerator
    {
        void GenerateTestDataFile(IExerciseTestIdentifiers cohort, FileInfo target, int numberOfRecords, IDataLoadEventListener listener);
    }

}
