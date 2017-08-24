using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HIC.Logging;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace RDMPObjectVisualisation.DataObjects
{
    /// <summary>
    /// Reusable icon for visualizing databases in the user interface
    /// </summary>
    public partial class DiscoveredDatabaseVisualisation : UserControl
    {
        private readonly DiscoveredDatabase _value;

        public DiscoveredDatabaseVisualisation(DiscoveredDatabase value)
        {
            _value = value;
            InitializeComponent();
        }

    }
}
