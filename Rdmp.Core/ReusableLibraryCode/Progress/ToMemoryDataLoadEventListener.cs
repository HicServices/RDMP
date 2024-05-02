// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rdmp.Core.ReusableLibraryCode.Progress;

/// <summary>
///     IDataLoadEventListener which records all OnNotify and all novel OnProgress messages in memory (in Dictionaries
///     where the key is component which
///     sent the message).  You can optionally respond to OnNotify events where the ProgressEventType is Error by throwing
///     an Exception.
///     <para>The typical use case for this is for testing to ensure that components log specific messages.</para>
/// </summary>
public class ToMemoryDataLoadEventListener : IDataLoadEventListener
{
    private readonly bool _throwOnErrorEvents;
    public Dictionary<object, List<NotifyEventArgs>> EventsReceivedBySender = new();
    public Dictionary<string, ProgressEventArgs> LastProgressRecieivedByTaskName = new();

    public ToMemoryDataLoadEventListener(bool throwOnErrorEvents)
    {
        _throwOnErrorEvents = throwOnErrorEvents;
    }

    public void OnNotify(object sender, NotifyEventArgs e)
    {
        if (e.ProgressEventType == ProgressEventType.Error && _throwOnErrorEvents)
            throw e.Exception ?? new Exception(e.Message);


        if (!EventsReceivedBySender.ContainsKey(sender))
            EventsReceivedBySender.Add(sender, new List<NotifyEventArgs>());

        EventsReceivedBySender[sender].Add(e);
    }

    public void OnProgress(object sender, ProgressEventArgs e)
    {
        LastProgressRecieivedByTaskName[e.TaskDescription] = e;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach (var kvp in EventsReceivedBySender)
        {
            sb.AppendLine($"{kvp.Key} Messages:");
            foreach (var msg in kvp.Value)
                sb.AppendLine($"{msg.ProgressEventType}:{msg.Message}");
        }

        foreach (var kvp in LastProgressRecieivedByTaskName)
            sb.AppendLine($"{kvp.Key} {kvp.Value.Progress.Value} {kvp.Value.Progress.UnitOfMeasurement}");

        return sb.ToString();
    }

    /// <summary>
    ///     Flattens EventsReceivedBySender and returns the result as a Dictionary by ProgressEventType (Error / Warning etc)
    /// </summary>
    /// <returns></returns>
    public Dictionary<ProgressEventType, List<NotifyEventArgs>> GetAllMessagesByProgressEventType()
    {
        var toReturn = new Dictionary<ProgressEventType, List<NotifyEventArgs>>();

        foreach (ProgressEventType e in Enum.GetValues(typeof(ProgressEventType)))
            toReturn.Add(e, new List<NotifyEventArgs>());

        foreach (var eventArgs in EventsReceivedBySender.Values.SelectMany(a => a))
            toReturn[eventArgs.ProgressEventType].Add(eventArgs);

        return toReturn;
    }

    public ProgressEventType GetWorst()
    {
        return EventsReceivedBySender.Values.Max(v => v.Max(e => e.ProgressEventType));
    }
}