﻿using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rdmp.UI.SimpleDialogs.Datasets
{
    public partial class CreateNewDatasetUI : RDMPForm
    {
        private IActivateItems _activator;
        public CreateNewDatasetUI(IActivateItems activator, ExecuteCommandCreateNewDatasetUI command) : base(activator)
        {
            _activator = activator;
            InitializeComponent();

        }

        private void CreateNewDatasetUI_Load(object sender, EventArgs e)
        {

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {

            var cmd = new ExecuteCommandCreateDataset(_activator,tbName.Text,tbDOI.Text,tbSource.Text);
            cmd.Execute();
            Close();
        }


    }
}