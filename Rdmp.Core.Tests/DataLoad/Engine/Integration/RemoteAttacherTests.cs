// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NuGet.Frameworks;
using NUnit.Framework;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Modules.Attachers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

public class RemoteAttacherTests
{

    [Test]
    public void TestRemoteAttacherParameterPast24Hrs()
    {
        var attacher = new RemoteAttacher();
        attacher.HistoricalFetchDuration = AttacherHistoricalDurations.Past24Hours;
        attacher.RemoteTableDateColumn = "date";
        var lmd = new LoadMetadata();
        Assert.That(attacher.SqlHistoricalDataFilter(lmd), Is.EqualTo($" WHERE CAST(date as Date) > dateadd(DAY, -1, GETDATE())"));
    }
    [Test]
    public void TestRemoteAttacherParameterPast7Days()
    {
        var attacher = new RemoteAttacher();
        attacher.HistoricalFetchDuration = AttacherHistoricalDurations.Past7Days;
        attacher.RemoteTableDateColumn = "date";
        var lmd = new LoadMetadata();
        Assert.That(attacher.SqlHistoricalDataFilter(lmd), Is.EqualTo($" WHERE CAST(date as Date) > dateadd(WEEK, -1, GETDATE())"));
    }
    [Test]
    public void TestRemoteAttacherParameterPastMonth()
    {
        var attacher = new RemoteAttacher();
        attacher.HistoricalFetchDuration = AttacherHistoricalDurations.PastMonth;
        attacher.RemoteTableDateColumn = "date";
        var lmd = new LoadMetadata();
        Assert.That(attacher.SqlHistoricalDataFilter(lmd), Is.EqualTo($" WHERE CAST(date as Date) > dateadd(MONTH, -1, GETDATE())"));
    }
    [Test]
    public void TestRemoteAttacherParameterPastYear()
    {
        var attacher = new RemoteAttacher();
        attacher.HistoricalFetchDuration = AttacherHistoricalDurations.PastYear;
        attacher.RemoteTableDateColumn = "date";
        var lmd = new LoadMetadata();
        Assert.That(attacher.SqlHistoricalDataFilter(lmd), Is.EqualTo($" WHERE CAST(date as Date) > dateadd(YEAR, -1, GETDATE())"));
    }
    [Test]
    public void TestRemoteAttacherParameterSinceLastUse()
    {
        var attacher = new RemoteAttacher();
        attacher.HistoricalFetchDuration = AttacherHistoricalDurations.SinceLastUse;
        attacher.RemoteTableDateColumn = "date";
        var lmd = new LoadMetadata();
        lmd.LastLoadTime = DateTime.Now;
        Assert.That(attacher.SqlHistoricalDataFilter(lmd), Is.EqualTo($" WHERE CAST(date as Date) > CAST('{lmd.LastLoadTime}' as Date)"));
    }
    [Test]
    public void TestRemoteAttacherParameterSinceLastUse_NULL()
    {
        var attacher = new RemoteAttacher();
        attacher.HistoricalFetchDuration = AttacherHistoricalDurations.SinceLastUse;
        attacher.RemoteTableDateColumn = "date";
        var lmd = new LoadMetadata();
        Assert.That(attacher.SqlHistoricalDataFilter(lmd), Is.EqualTo($""));
    }
    [Test]
    public void TestRemoteAttacherParameterCustomRange()
    {
        var attacher = new RemoteAttacher();
        attacher.HistoricalFetchDuration = AttacherHistoricalDurations.Custom;
        attacher.RemoteTableDateColumn = "date";
        attacher.CustomFetchDurationStartDate = DateTime.Now;
        attacher.CustomFetchDurationEndDate = DateTime.Now;
        var lmd = new LoadMetadata();
        lmd.LastLoadTime = DateTime.Now;
        Assert.That(attacher.SqlHistoricalDataFilter(lmd), Is.EqualTo($" WHERE CAST(date as Date) >= CAST('{attacher.CustomFetchDurationStartDate}' as Date) AND CAST(date as Date) <= CAST('{attacher.CustomFetchDurationEndDate}' as Date)"));
    }
    [Test]
    public void TestRemoteAttacherParameterCustomRangeNoStart()
    {
        var attacher = new RemoteAttacher();
        attacher.HistoricalFetchDuration = AttacherHistoricalDurations.Custom;
        attacher.RemoteTableDateColumn = "date";
        attacher.CustomFetchDurationEndDate = DateTime.Now;
        var lmd = new LoadMetadata();
        lmd.LastLoadTime = DateTime.Now;
        Assert.That(attacher.SqlHistoricalDataFilter(lmd), Is.EqualTo($" WHERE CAST(date as Date) <= CAST('{attacher.CustomFetchDurationEndDate}' as Date)"));
    }
    [Test]
    public void TestRemoteAttacherParameterCustomRangeNoEnd()
    {
        var attacher = new RemoteAttacher();
        attacher.HistoricalFetchDuration = AttacherHistoricalDurations.Custom;
        attacher.RemoteTableDateColumn = "date";
        attacher.CustomFetchDurationStartDate = DateTime.Now;
        var lmd = new LoadMetadata();
        lmd.LastLoadTime = DateTime.Now;
        Assert.That(attacher.SqlHistoricalDataFilter(lmd), Is.EqualTo($" WHERE CAST(date as Date) >= CAST('{attacher.CustomFetchDurationStartDate}' as Date)"));
    }
    [Test]
    public void TestRemoteAttacherParameterCustomRangeNoDates()
    {
        var attacher = new RemoteAttacher();
        attacher.HistoricalFetchDuration = AttacherHistoricalDurations.Custom;
        attacher.RemoteTableDateColumn = "date";
        var lmd = new LoadMetadata();
        lmd.LastLoadTime = DateTime.Now;
        Assert.That(attacher.SqlHistoricalDataFilter(lmd), Is.EqualTo(""));
    }
    [Test]
    public void TestRemoteAttacherParameterForwardScan()
    {
        var attacher = new RemoteAttacher();
        attacher.HistoricalFetchDuration = AttacherHistoricalDurations.ForwardScan;
        attacher.ForwardScanLookBackDays = 1;
        attacher.ForwardScanLookForwardDays = 1;
        attacher.ForwardScanDateInTime = DateTime.Now;
        attacher.RemoteTableDateColumn = "date";
        var lmd = new LoadMetadata();
        lmd.LastLoadTime = DateTime.Now;
        Assert.That(attacher.SqlHistoricalDataFilter(lmd), Is.EqualTo($" WHERE CAST(date as Date) >= CAST('{attacher.ForwardScanDateInTime.AddDays(-attacher.ForwardScanLookBackDays)}' as Date) AND CAST(date as Date) <= CAST('{attacher.ForwardScanDateInTime.AddDays(attacher.ForwardScanLookForwardDays)}' as Date)"));
    }
    [Test]
    public void TestRemoteAttacherParameterForwardScan_NoLookBack()
    {
        var attacher = new RemoteAttacher();
        attacher.HistoricalFetchDuration = AttacherHistoricalDurations.ForwardScan;
        attacher.ForwardScanLookForwardDays = 1;
        attacher.ForwardScanDateInTime = DateTime.Now;
        attacher.RemoteTableDateColumn = "date";
        var lmd = new LoadMetadata();
        lmd.LastLoadTime = DateTime.Now;
        Assert.That(attacher.SqlHistoricalDataFilter(lmd), Is.EqualTo($" WHERE CAST(date as Date) >= CAST('{attacher.ForwardScanDateInTime}' as Date) AND CAST(date as Date) <= CAST('{attacher.ForwardScanDateInTime.AddDays(attacher.ForwardScanLookForwardDays)}' as Date)"));
    }
    [Test]
    public void TestRemoteAttacherParameterForwardScan_NoLookForward()
    {
        var attacher = new RemoteAttacher();
        attacher.HistoricalFetchDuration = AttacherHistoricalDurations.ForwardScan;
        attacher.ForwardScanLookBackDays = 1;
        attacher.ForwardScanDateInTime = DateTime.Now;
        attacher.RemoteTableDateColumn = "date";
        var lmd = new LoadMetadata();
        lmd.LastLoadTime = DateTime.Now;
        Assert.That(attacher.SqlHistoricalDataFilter(lmd), Is.EqualTo($" WHERE CAST(date as Date) >= CAST('{attacher.ForwardScanDateInTime.AddDays(-attacher.ForwardScanLookBackDays)}' as Date) AND CAST(date as Date) <= CAST('{attacher.ForwardScanDateInTime}' as Date)"));
    }
    [Test]
    public void TestRemoteAttacherParameterForwardScanNoDates()
    {
        var attacher = new RemoteAttacher();
        attacher.HistoricalFetchDuration = AttacherHistoricalDurations.ForwardScan;
        attacher.RemoteTableDateColumn = "date";
        var lmd = new LoadMetadata();
        Assert.That(attacher.SqlHistoricalDataFilter(lmd), Is.EqualTo(""));
    }
}
