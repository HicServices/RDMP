using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using FAnsi.Discovery;
using FAnsi.Discovery.TableCreation;

namespace LoadModules.GenericUIs
{
    [Export(typeof(IDatabaseColumnRequestAdjuster))]
    public partial class AdjustColumnDataTypesUI : Form, IDatabaseColumnRequestAdjuster
    {
        private List<DatabaseColumnRequest> _columns;

        public AdjustColumnDataTypesUI()
        {
            InitializeComponent();
        }

        public void AdjustColumns(List<DatabaseColumnRequest> columns)
        {
            _columns = columns;

            foreach (DatabaseColumnRequest column in _columns)
            {
                var ui = new DatabaseColumnRequestUI(column);
                ui.Dock = DockStyle.Top;
                flowLayoutPanel1.Controls.Add(ui);
            }


            ShowDialog();
        }

        private void btnDone_Click(object sender, System.EventArgs e)
        {
            if (_columns == null)
                throw new Exception("AdjustColumns was not called yet");
            
            Close();
        }
    }
}
