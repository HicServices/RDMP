using NUnit.Framework;
using Rdmp.Core.Logging.PastEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Tests.Logging
{
    class ArchivalDataLoadInfoTests
    {
        [Test]
        public void ArchivalDataLoadInfo_ToString()
        {
            var adi = new ArchivalDataLoadInfo();
            adi.StartTime = new DateTime(2010, 1, 1,12,0,0);
            adi.EndTime = new DateTime(2010, 1, 3,13,20,23);

            // This dle took 2 days, 1 hour, 20 mins and 23 seconds
            StringAssert.Contains("(49:20:23)", adi.ToString());
        }
    }
}
