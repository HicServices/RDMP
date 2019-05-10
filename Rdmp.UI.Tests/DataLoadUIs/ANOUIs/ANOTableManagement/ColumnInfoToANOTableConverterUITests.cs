using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.DataLoadUIs.ANOUIs.ANOTableManagement;

namespace Rdmp.UI.Tests.DataLoadUIs.ANOUIs.ANOTableManagement
{
	public class ColumnInfoToANOTableConverterUITests :UITests
	{
		[Test,UITimeout(20000)]
		public void Test_ColumnInfoToANOTableConverterUI_Constructor()
		{
			var o = WhenIHaveA<ColumnInfo>();
			var ui = AndLaunch<ColumnInfoToANOTableConverterUI>(o);
			Assert.IsNotNull(ui);
			//AssertNoErrors(ExpectedErrorType.Fatal);
			//AssertNoErrors(ExpectedErrorType.KilledForm);
		}
	}
}
