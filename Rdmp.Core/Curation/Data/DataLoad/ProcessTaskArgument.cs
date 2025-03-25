// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.Data.DataLoad;

/// <summary>
///     Contains a strongly typed value which will be provided to an instantiated data class of ProcessTask at runtime.
///     These usually correspond
///     1 to 1 with [DemandsInitialization] flagged properties of a data class e.g.
///     'LoadModules.Generic.Attachers.AnySeparatorFileAttacher' would have
///     a ProcessTaskArgument record for the property UnderReadBehaviour and one for IgnoreBlankLines etc.
///     <para>
///         This all happens transparently by reflection and is handled at design time through PluginProcessTaskUI
///         seamlessly
///     </para>
/// </summary>
public sealed class ProcessTaskArgument : Argument
{
    #region Database Properties

    private int _processTask_ID;

    /// <summary>
    ///     The task for which this <see cref="ProcessTaskArgument" /> stores values
    /// </summary>
    [Relationship(typeof(ProcessTask), RelationshipType.SharedObject)]
    public int ProcessTask_ID
    {
        get => _processTask_ID;
        set => SetField(ref _processTask_ID, value);
    }

    #endregion

    #region Relationships

    /// <inheritdoc cref="ProcessTask_ID" />
    [NoMappingToDatabase]
    public ProcessTask ProcessTask => Repository.GetObjectByID<ProcessTask>(ProcessTask_ID);

    #endregion

    public ProcessTaskArgument()
    {
    }

    /// <summary>
    ///     Stores a new argument value for the class hosted by <see cref="ProcessTask" />. Use
    ///     <see cref="ArgumentFactory" /> if you want to do this in a more structured manner.
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="parent"></param>
    public ProcessTaskArgument(ICatalogueRepository repository, ProcessTask parent)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "ProcessTask_ID", parent.ID },
            { "Name", $"Parameter{Guid.NewGuid()}" },
            { "Type", typeof(string).ToString() }
        });
    }

    internal ProcessTaskArgument(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        ProcessTask_ID = int.Parse(r["ProcessTask_ID"].ToString());
        Type = r["Type"].ToString();
        Name = r["Name"].ToString();
        Value = r["Value"] as string;
        Description = r["Description"] as string;
    }

    internal ProcessTaskArgument(ShareManager shareManager, ShareDefinition shareDefinition)
    {
        shareManager.UpsertAndHydrate(this, shareDefinition);
        try
        {
            //if the import is into a repository other than the master original repository
            if (!shareManager.IsExportedObject(ProcessTask.LoadMetadata))
            {
                //and we are a reference type e.g. to a ColumnInfo or something
                var t = GetConcreteSystemType();

                if (typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(t) ||
                    typeof(IEnumerable<IMapsDirectlyToDatabaseTable>).IsAssignableFrom(t))
                {
                    //then use the value Null because whatever ID is stored in us won't be pointing to the same object
                    //as when we were exported!
                    Value = null;
                    SaveToDatabase();
                }
            }
        }
        catch (Exception e)
        {
            //couldn't work out the Type, maybe it is broken or something, or otherwise someone elses problem
            Console.WriteLine(e);
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }

    /// <summary>
    ///     Creates new ProcessTaskArguments for the supplied class T (based on what DemandsInitialization fields it has).
    ///     Parent is the ProcessTask that hosts the class T e.g. IAttacher
    /// </summary>
    /// <typeparam name="T">A class that has some DemandsInitializations</typeparam>
    /// <param name="parent"></param>
    public static IArgument[] CreateArgumentsForClassIfNotExists<T>(IProcessTask parent)
    {
        return ArgumentFactory.CreateArgumentsForClassIfNotExistsGeneric<T>(
                //tell it how to create new instances of us related to parent
                parent,

                //what arguments already exist
                parent.GetAllArguments().ToArray())

            //convert the result back from generic to specific (us)
            .ToArray();
    }

    public ProcessTaskArgument ShallowClone(ProcessTask into)
    {
        var clone = new ProcessTaskArgument(CatalogueRepository, into);
        CopyShallowValuesTo(clone, true);

        return clone;
    }
}