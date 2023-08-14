// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
namespace Rdmp.Core.Logging;


/// <summary>
/// Internal Object used to store log entries while there are inthe queue for batch insert into a database
/// Currently used for batching of the progressLog logs
/// </summary>
public class LogEntry
{
    public LogEntry(string eventType, string description, string source, DateTime time)
    {
        EventType = eventType;
        Description = description;
        Time = time;
        Source = source;
    }

    public string EventType { get; private set; }
    public string Description { get; private set; }
    public DateTime Time { get; private set; }
    public string Source { get; private set; }
}