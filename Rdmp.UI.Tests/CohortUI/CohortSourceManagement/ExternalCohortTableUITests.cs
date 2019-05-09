using NUnit.Framework;
using Rdmp.Core.DataExport.Data;
using Rdmp.UI.CohortUI.CohortSourceManagement;

namespace Rdmp.UI.Tests.CohortUI.CohortSourceManagement
{
	public class ExternalCohortTableUITests :UITests
	{
		[Test,UITimeout(20000)]
		public void Test_ExternalCohortTableUI_Constructor()
		{
			var o = WhenIHaveA<ExternalCohortTable>();
			var ui = AndLaunch<ExternalCohortTableUI>(o);
			Assert.IsNotNull(ui);

            //because cohort table doesnt actually go to a legit database the source should have been blacklisted during the child provider stage (not really related to our UI).
			AssertErrorWasShown(ExpectedErrorType.Fatal,"Blacklisted source 'My cohorts'"); 
			AssertNoErrors(ExpectedErrorType.KilledForm);
            AssertNoErrors(ExpectedErrorType.ErrorProvider);
            AssertNoErrors(ExpectedErrorType.FailedCheck); //checks are not run until user manually runs them in this UI
		}
	}
}