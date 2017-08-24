using System;
using System.Windows.Forms;
using CatalogueLibrary.Data.Aggregation;
using CohortManagerLibrary.QueryBuilding;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;

namespace RDMPObjectVisualisation.DataObjects
{
    /// <summary>
    /// TECHNICAL: Allows visualisation of a 'patient index table'.  Patient index tables are AggregateConfigurations that are used in cohort generation (in CohortManager).  They are queries
    /// which when executed will return a table of relevant events about a patient set (e.g. all the prescription dates for opiates in Tayside).  This table will be part of a cohort identification
    /// configuration (e.g. find patients who have had a prescription for cannabis within 3 weeks of a prescription for opiates and are alive today and resident in Tayside).
    /// 
    /// If you are seeing a PatientIndexTableVisualisation then it means that a patient index table query is available for use in the pipeline you are executing.  For example you might have executed
    /// a cohort identification configuration and imported it as a cohort and now chosen to import all of the patient index tables used to give them to the researcher as custom data in thier project
    /// extract.
    /// </summary>
    public partial class PatientIndexTableVisualisation : UserControl, ICheckable
    {
        private readonly AggregateConfiguration _configuration;

        public PatientIndexTableVisualisation(AggregateConfiguration configuration)
        {
            _configuration = configuration;
            InitializeComponent();
            DoTransparencyProperly.ThisHoversOver(ragSmiley1,pictureBox1);

            if(_configuration !=  null)
                this.Check(ragSmiley1);
        }


        public void Check(ICheckNotifier notifier)
        {
            var cic = _configuration.GetCohortIdentificationConfigurationIfAny();

            if (cic == null)
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "AggregateConfiguration " + _configuration +
                        " is not a joinable Patient Index Table because it does not have any CohortIdentificationConfiguration associated with it",
                        CheckResult.Fail));

                return;
            }

            try
            {
                CohortQueryBuilder builder = new CohortQueryBuilder(_configuration,cic.GetAllParameters(),true);
                notifier.OnCheckPerformed(new CheckEventArgs("Generated the following SQL for the PatientIndexTableVisualisation:" +Environment.NewLine + builder.SQL,CheckResult.Success));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "Failed to generate query for AggregateConfiguration (patient index table) " + _configuration,
                        CheckResult.Fail, e));
            }

        }
    }
}
