using System;
using System.Collections.Generic;
using CatalogueLibrary.Data;
using HIC.Common.Validation;
using NUnit.Framework;

namespace CatalogueLibraryTests.Unit
{
    [Category("Unit")]
    public class PermissionWindowTests
    {

        [Test]
        public void TestSerialisation()
        {
            var period1 = new PermissionWindowPeriod((int) DayOfWeek.Monday, new TimeSpan(0, 0, 0), new TimeSpan(6, 0, 0));
            var period2 = new PermissionWindowPeriod((int) DayOfWeek.Monday, new TimeSpan(17, 0, 0), new TimeSpan(23, 59, 59));

            var permissionWindow = new PermissionWindow(new List<PermissionWindowPeriod>
            {
                period1,
                period2
            });

            var permissionPeriodConfig = permissionWindow.PermissionPeriodConfig;

            var newPermissionWindow = new PermissionWindow();
            newPermissionWindow.PermissionPeriodConfig = permissionPeriodConfig;

            var periods = newPermissionWindow.PermissionWindowPeriods;
            Assert.AreEqual(2, periods.Count);

            var newPeriod1 = periods[0];
            Assert.AreEqual((int) DayOfWeek.Monday, newPeriod1.DayOfWeek);

            Assert.AreEqual(6, newPeriod1.End.Hours);

            var newPeriod2 = periods[1];
            Assert.AreEqual(17, newPeriod2.Start.Hours);
        }
        
        [Test]
        public void TestCurrentlyWithinPermissionPeriod()
        {
            var dtNow = DateTime.UtcNow;

            if ((dtNow.Hour == 23 && dtNow.Minute >= 40) || (dtNow.Hour == 0 && dtNow.Minute <= 5))
                Assert.Inconclusive("This test cannot run at midnight since it is afraid of the dark");
            
            var fiveMinutes = new TimeSpan(0, 5, 0);

            var utcTime = new TimeSpan(dtNow.Hour, dtNow.Minute, dtNow.Second);
            var period1 = new PermissionWindowPeriod((int)DateTime.Now.DayOfWeek, utcTime.Subtract(fiveMinutes), utcTime.Add(fiveMinutes));

            var permissionWindow = new PermissionWindow(new List<PermissionWindowPeriod> {period1});
            Assert.IsTrue(permissionWindow.CurrentlyWithinPermissionWindow());
        }

        [Test]
        public void TestCurrentlyOutsidePermissionPeriod()
        {
            var dtNow = DateTime.UtcNow;

            if ((dtNow.Hour == 23 && dtNow.Minute >= 50) || (dtNow.Hour == 0 && dtNow.Minute <= 3))
                Assert.Inconclusive("This test cannot run at midnight since it is afraid of the dark");
            
            var oneMinute = new TimeSpan(0, 1, 0);
            var utcTime = new TimeSpan(dtNow.Hour, dtNow.Minute, dtNow.Second);
            var period1 = new PermissionWindowPeriod((int)DateTime.Now.DayOfWeek, utcTime.Add(oneMinute), utcTime.Add(oneMinute));

            var permissionWindow = new PermissionWindow(new List<PermissionWindowPeriod> { period1 });
            Assert.IsFalse(permissionWindow.CurrentlyWithinPermissionWindow());
        }
    }
}
