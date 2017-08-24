using System;
using System.Diagnostics;
using System.IO;
using ReusableLibraryCode.Progress;

namespace Diagnostics.TestData.Exercises
{
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

        protected abstract object[] GenerateTestDataRow(TestPerson p);
        protected abstract void WriteHeaders(StreamWriter sw);
    }
}