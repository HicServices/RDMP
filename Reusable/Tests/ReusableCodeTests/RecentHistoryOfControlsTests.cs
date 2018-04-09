using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NUnit.Framework;
using ReusableUIComponents;

namespace ReusableCodeTests
{
    public class RecentHistoryOfControlsTests
    {
        [Test]
        public void TestOverflowPrevention()
        {
            var t = new TextBox();

            var c = new RecentHistoryOfControls(t,new Guid("b3ccaf14-702a-438a-8cf4-d550d6d7775d"));

            c.Clear();
            int overFlowCounter = 100000;

            for (int i = 0; i < overFlowCounter; i++)
                c.AddResult("testOverflowValue" + Guid.NewGuid(), i%1000 == 0);//only save every X values added for performance

            c.Clear();
        }

    }
}