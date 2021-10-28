// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TypeGuesser.Deciders;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{

    /// <summary>
    /// Checks the RDMP logs for the latest log entry of a given object.  Throws (returns exit code non zero) if
    /// the top log entry is failing or if there are no log entries within the expected time span.
    /// </summary>
    public class ExecuteCommandConfirmLogs : BasicCommandExecution {

        /// <summary>
        /// Optional time period in which to expect successful logs
        /// </summary>
        TimeSpan? WithinTime { get; set; }

        /// <summary>
        /// The object which generates logs that you want to check
        /// </summary>
        public ILoggedActivityRootObject LogRootObject { get; }

        /// <summary>
        /// Checks the RDMP logs for the latest log entry of a given object.  Throws (returns exit code non zero) if
        /// the top log entry is failing or if there are no log entries within the expected time span.
        /// </summary>
        /// <param name="activator"></param>
        /// <param name="obj"></param>
        /// <param name="withinTime"></param>
        public ExecuteCommandConfirmLogs(IBasicActivateItems activator,
            
            [DemandsInitialization("The object you want to confirm passing log entries for")]
            ILoggedActivityRootObject obj,

            [DemandsInitialization("Optional time period in which to expect successful logs e.g. 24:00:00 (24 hours)")]
            string withinTime = null):base(activator)
        {
            LogRootObject = obj;
            if(withinTime != null)
            {
                var decider = new TimeSpanTypeDecider(CultureInfo.CurrentCulture);
                WithinTime = (TimeSpan)decider.Parse(withinTime);
            }
        }

        public override void Execute()
        {
            base.Execute();

            var logManager = new LogManager(LogRootObject.GetDistinctLoggingDatabase());

            // get the latest log entry
            var latest = logManager.GetArchivalDataLoadInfos(LogRootObject.GetDistinctLoggingTask(), null, null, 1).SingleOrDefault();

            // if no logs
            if(latest == null)
            {
                throw new LogsNotConfirmedException($"There are no log entries for {LogRootObject}");
            }

            // we have logs but are they in the time period we are interested in
            if (WithinTime.HasValue)
            {
                var thresholdDate = DateTime.Now.Subtract(WithinTime.Value);
                var startTime = latest.StartTime;

                // if the latest log entry is older than the time period the user indicated
                if (startTime < thresholdDate)
                {
                    throw new LogsNotConfirmedException($"Latest logged activity for {LogRootObject} is {startTime}.  This is older than the requested date threshold:{thresholdDate}");
                }
            }

            // we have an acceptably recent log entry
            if(latest.HasErrors)
            {
                throw new LogsNotConfirmedException($"Latest logs for {LogRootObject} ({latest.StartTime}) indicate that it failed");
            }

            // most recent log entry did not complete
            if (!latest.EndTime.HasValue)
            {
                throw new LogsNotConfirmedException($"Latest logs for {LogRootObject} ({latest.StartTime}) indicate that it did not complete");
            }
            // latest log entry is passing yay!
        }

        /// <summary>
        /// Thrown when <see cref="ExecuteCommandConfirmLogs"/> identifies that the expected logged activities are not present or indicate failure
        /// </summary>
        [Serializable]
        public class LogsNotConfirmedException : Exception
        {
            public LogsNotConfirmedException()
            {
            }

            public LogsNotConfirmedException(string message) : base(message)
            {
            }

            public LogsNotConfirmedException(string message, Exception innerException) : base(message, innerException)
            {
            }

            protected LogsNotConfirmedException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }
    }
}
