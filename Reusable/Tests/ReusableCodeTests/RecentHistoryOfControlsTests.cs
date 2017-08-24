using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ReusableUIComponents;

namespace ReusableCodeTests
{
    public class RecentHistoryOfControlsTests
    {

        [Test]
        public void TestSerialization()
        {
            RecentHistoryOfControls.RDMPRegistryRootKey = @"HKEY_CURRENT_USER\Software\HICDataManagementPlatform";
            RecentHistoryOfControls.GetInstance().AddResults("test", "testva");
            Assert.IsTrue(RecentHistoryOfControls.GetInstance().GetList("test").Contains("testva"));
            Assert.IsNull(RecentHistoryOfControls.GetInstance().GetList("FishesBeFlying"));
        }


        [Test]
        public void TestOverflowPrevention()
        {
            RecentHistoryOfControls.RDMPRegistryRootKey = @"HKEY_CURRENT_USER\Software\HICDataManagementPlatform";
            RecentHistoryOfControls.GetInstance().Clear();
            int overFlowCounter = 100000;

            for (int i = 0; i < overFlowCounter; i++)
            {
                bool shouldSave = i % 1000 == 0;
                bool result = RecentHistoryOfControls.GetInstance().AddResults("testOverflow", Guid.NewGuid().ToString(), shouldSave);//only save every X values added for performance

                if (shouldSave != result)
                    break;
            }

            List<string> vals = RecentHistoryOfControls.GetInstance().GetList("testOverflow");

            Assert.LessOrEqual(vals.Count(), overFlowCounter);
            Console.WriteLine("Overflow prevented at " + vals.Count() + " autocomplete values");

            //cleanup by clearing it
            RecentHistoryOfControls.GetInstance().Clear();
        }

    }
}