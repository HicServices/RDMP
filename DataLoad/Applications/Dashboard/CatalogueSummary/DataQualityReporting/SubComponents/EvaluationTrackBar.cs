using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DataQualityEngine.Data;

namespace Dashboard.CatalogueSummary.DataQualityReporting.SubComponents
{
    /// <summary>
    /// The Data Quality Engine stores all validation results in a relational database.  This includes the time the DQE was run.  This allows us to 'rewind' and look at previous results
    /// e.g. to compare the quality of the dataset before and after a data load.
    /// 
    /// If this control is not enabled then it means you have only ever done one DQE evaluation or have never evaluated the dataset by using the DQE.
    /// 
    /// Dragging the slider will adjust the TimePeriodicityChart, ColumnStatesChart and RowStatePieChart to show the results of the DQE on that day.
    /// </summary>
    public partial class EvaluationTrackBar : UserControl
    {
        private Evaluation[] _evaluations;

        public EvaluationTrackBar()
        {
            InitializeComponent();
        }


        public Evaluation[] Evaluations
        {
            get { return _evaluations; }
            set
            {
                _evaluations = value;
                RefreshUI();
            }
        }

        private List<Label> labels = new List<Label>();
        public event EvaluationSelectedHandler EvaluationSelected;

        private void RefreshUI()
        {
            if (Evaluations == null || Evaluations.Length == 0)
            {
                this.Enabled = false;
                return;
            }
            else if (Evaluations.Length == 1)
            {
                //there is only 1 
                Enabled = false;
                EvaluationSelected(this, Evaluations.Single());
            }
            else
                this.Enabled = true;//let user drag around the trackbar if he wants

            foreach (Label label in labels)
            {
                this.Controls.Remove(label);
                label.Dispose();
            }
            labels.Clear();
            
            //if there is at least 2 evaluations done then we need to have a track bar of evaluations
            tbEvaluation.Minimum = 0;
            tbEvaluation.Maximum = Evaluations.Length - 1;
            tbEvaluation.TickFrequency = 1;
            tbEvaluation.Value = Evaluations.Length - 1;
            tbEvaluation.LargeChange = 1;

            for (int i = 0; i < Evaluations.Length; i++)
            {
                double ratio = ((double)i)/(Evaluations.Length-1);


                int x = tbEvaluation.Left + (int) (ratio * tbEvaluation.Width);
                int y = tbEvaluation.Bottom - 10;

                Label l = new Label();
                l.Text = Evaluations[i].DateOfEvaluation.ToString("d");
                l.Location = new Point(x - (l.PreferredWidth / 2), y);
              
                this.Controls.Add(l);
                l.BringToFront();
                
                labels.Add(l);
                
            }
            
            tbEvaluation.Value = tbEvaluation.Maximum;
            EvaluationSelected(this, Evaluations[tbEvaluation.Value]);
        }

        private void tbEvaluation_ValueChanged(object sender, EventArgs e)
        {
            if (tbEvaluation.Value >= 0)
                EvaluationSelected(this,Evaluations[tbEvaluation.Value]);
        }
    }

    public delegate void EvaluationSelectedHandler(object sender, Evaluation evaluation);
}
