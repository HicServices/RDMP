using System;
using System.Windows.Forms;
using CatalogueLibrary.Data.Aggregation;
using CohortManagerLibrary.QueryBuilding;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;

namespace RDMPObjectVisualisation.DataObjects
{
    /// <summary>
    /// TECHNICAL: Allows visualisation of a <see cref="AggregateConfiguration"/> which might be a cohort set, graph of records or 'patient index table'.
    /// 
    /// <para>
    /// Patient index tables are AggregateConfigurations that are used in cohort generation (in CohortManager).  They are queries which when executed will return a table of relevant 
    /// events about a patient set (e.g. all the prescription dates for opiates in Tayside).  This table will be part of a cohort identification configuration (e.g. find patients who
    /// have had a prescription for cannabis within 3 weeks of a prescription for opiates and are alive today and resident in Tayside).
    /// </para>
    /// </summary>
    public partial class AggregateConfigurationVisualisation : UserControl
    {
        private readonly AggregateConfiguration _configuration;

        public AggregateConfigurationVisualisation(AggregateConfiguration configuration)
        {
            _configuration = configuration;
            InitializeComponent();
            
            if (_configuration.IsJoinablePatientIndexTable())
                pictureBox1.Image = Images.BigPatientIndexTable;
            else if (_configuration.IsCohortIdentificationAggregate)
                pictureBox1.Image = Images.BigCohort;
            else
                pictureBox1.Image = Images.BigGraph;
        }
    }
}
