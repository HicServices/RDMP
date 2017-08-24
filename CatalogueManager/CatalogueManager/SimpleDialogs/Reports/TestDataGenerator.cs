using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Diagnostics.TestData.Exercises;
using ReusableLibraryCode.Progress;

namespace CatalogueManager.SimpleDialogs.Reports
{
    /// <summary>
    /// Part of UserExercisesUI  (See UserExercisesUI).  This control lets you decide how many records in the dataset to create.  This data is fictional although it is designed to look
    /// semi real and exhibit peculiarities common to medical records.  The slider is exponential so if you drag it all the way to the top expect to wait for a weekend for it to generate
    /// all the data.
    /// </summary>
    public partial class TestDataGenerator : UserControl, IDataLoadEventListener
    {

        public TestDataGenerator()
        {
            InitializeComponent();
            trackBar1.LargeChange = 1;
        }

        public IExerciseTestDataGenerator Generator { get; set; }

        public int GetSize()
        {
            return 10* (int)Math.Pow(10, trackBar1.Value);
        }

        int sizeAtBeginGeneration = -1;
        public Thread Thread;
        public event Action TrackBarMouseUp;
        public event Action Completed;

        public void BeginGeneration(IExerciseTestIdentifiers cohort, FileInfo target)
        {
            //already running
            if(sizeAtBeginGeneration != -1)
                return;

            sizeAtBeginGeneration = GetSize();

            Thread = new Thread(() => Generator.GenerateTestDataFile(cohort, target, sizeAtBeginGeneration, this));
            Thread.Start();

        }

        public void OnNotify(object sender, NotifyEventArgs e)
        {
            
        }

        public void OnProgress(object sender, ProgressEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => OnProgress(sender, e)));
                return;
            }

            double percentProgress = e.Progress.Value/(double)sizeAtBeginGeneration * 100.0;
            progressBar1.Value = (int)percentProgress;

            if (progressBar1.Value == 100)
                if (Completed != null)
                    Completed();
        }

        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            if (TrackBarMouseUp != null)
                TrackBarMouseUp();
        }
    }
}
