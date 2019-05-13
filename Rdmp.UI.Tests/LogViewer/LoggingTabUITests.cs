using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.LogViewer;

namespace Rdmp.UI.Tests.LogViewer
{
	public class LoggingTabUITests :UITests
	{
		[Test,UITimeout(20000)]
		public void Test_LoggingTabUI_Constructor()
		{
			var o = WhenIHaveA<ExternalDatabaseServer>();
			var ui = AndLaunch<LoggingTabUI>(o);
			Assert.IsNotNull(ui);
		    AssertErrorWasShown(ExpectedErrorType.KilledForm,"Database My Server did not exist");
		}
	}
}