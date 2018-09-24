using System;
using System.Collections.Generic;
using CatalogueLibrary.Data.Cohort;
using MapsDirectlyToDatabaseTable.Revertable;

namespace CatalogueLibrary.Data.DataLoad
{
    /// <inheritdoc cref="ProcessTask"/>
    public interface IProcessTask : IRevertable, IArgumentHost, ILoadProgressHost, IOrderable
    {
        /// <inheritdoc cref="IArgumentHost.GetAllArguments"/>
        IEnumerable<ProcessTaskArgument> ProcessTaskArguments { get; }

        /// <summary>
        /// Either the C# Type name of a data load component (e.g. an IAttatcher, IDataProvider) or the path to an sql file or exe file (depending on <see cref="ProcessTaskType"/>)
        /// </summary>
        string Path { get; }

        /// <summary>
        /// The human readable description of what the component is supposed to do (e.g. "Copy all csv files from c:/temp/landing into ForLoading")
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The stage of the data load (RAW=>STAGING=>LIVE) that the task should be run at.  This can restrict which operations are allowed e.g. you can't run attatchers PostLoad
        /// </summary>
        LoadStage LoadStage { get; }

        /// <inheritdoc cref="CatalogueLibrary.Data.DataLoad.ProcessTaskType"/>
        ProcessTaskType ProcessTaskType { get; }

        
        /// <summary>
        /// Allows you to specify that a task should only be run when loading a specific <see cref="Catalogue"/>.  Since you can't change which <see cref="Catalogue"/> are loaded
        /// by a <see cref="LoadMetadata"/> at runtime, this property is now obsolete
        /// </summary>
        [Obsolete("Since you can't change which Catalogues are loaded by a LoadMetadata at runtime, this property is now obsolete")]
        int? RelatesSolelyToCatalogue_ID { get; }
        
        /// <summary>
        /// True to skip the <see cref="ProcessTask"/> when executing the data load
        /// </summary>
        bool IsDisabled { get; }
    }
}