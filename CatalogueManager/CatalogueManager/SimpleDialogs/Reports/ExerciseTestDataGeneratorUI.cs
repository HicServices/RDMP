// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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
    /// Part of GenerateTestDataUI  (See GenerateTestDataUI).  This control lets you decide how many records in the dataset to create.  This data is fictional although it is designed to look
    /// semi real and exhibit peculiarities common to medical records.  The slider is exponential so if you drag it all the way to the top expect to wait for a weekend for it to generate
    /// all the data.
    /// </summary>
    public partial class ExerciseTestDataGeneratorUI : UserControl, IDataLoadEventListener
    {

        public ExerciseTestDataGeneratorUI()
        {
            InitializeComponent();
            trackBar1.LargeChange = 1;
        }

        public IExerciseTestDataGenerator Generator
        {
            get { return _generator; }
            set
            {
                _generator = value;
                lblName.Text = value != null ? value.GetName():"";
            }
        }

        public int GetSize()
        {
            return 10* (int)Math.Pow(10, trackBar1.Value);
        }

        int sizeAtBeginGeneration = -1;
        public Thread Thread;
        private IExerciseTestDataGenerator _generator;
        public event Action TrackBarMouseUp;
        public event Action Completed;

        public void BeginGeneration(IExerciseTestIdentifiers cohort, DirectoryInfo target)
        {
            //already running
            if(sizeAtBeginGeneration != -1)
                return;

            sizeAtBeginGeneration = GetSize();

            var fi = new FileInfo(Path.Combine(target.FullName, Generator.GetName() + ".csv"));

            Thread = new Thread(() => Generator.GenerateTestDataFile(cohort, fi, sizeAtBeginGeneration, this));
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
