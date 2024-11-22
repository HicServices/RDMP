// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandShow : BasicCommandExecution
{
    private IMapsDirectlyToDatabaseTable _objectToShow;

    /// <summary>
    /// Alternative to <see cref="_objectToShow"/> where there might be many objects and it could be expensive to fetch them so only do so when command is executed
    /// </summary>
    private Func<IEnumerable<IMapsDirectlyToDatabaseTable>> _getObjectsFunc;

    private IMapsDirectlyToDatabaseTable[] _objectsToPickFrom;


    private readonly int _expansionDepth;
    public bool UseIconAndTypeName;
    private Type _objectType;

    public ExecuteCommandShow(IBasicActivateItems activator, IMapsDirectlyToDatabaseTable objectToShow,
        int expansionDepth, bool useIconAndTypeName = false) : base(activator)
    {
        _objectToShow = objectToShow;
        _objectType = _objectToShow?.GetType();
        _expansionDepth = expansionDepth;
        UseIconAndTypeName = useIconAndTypeName;

        if (_objectToShow == null)
            SetImpossible("No objects found");

        Weight = 50.3f;
    }


    public ExecuteCommandShow(IBasicActivateItems activator,
        IEnumerable<IMapsDirectlyToDatabaseTable> objectsToPickFrom, int expansionDepth,
        bool useIconAndTypeName = false) : base(activator)
    {
        if (objectsToPickFrom == null)
        {
            SetImpossible("No objects found");
            return;
        }

        var obs = objectsToPickFrom.ToArray();

        switch (obs.Length)
        {
            //no objects!
            case 0:
                SetImpossible("No objects found");
                break;
            case 1:
                //one object only
                _objectToShow = obs.Single();
                _objectType = _objectToShow.GetType();
                break;
            default:
                //many objects, let's assume they are of the same type for display purposes
                _objectType = obs.First().GetType();
                _objectsToPickFrom = obs;
                break;
        }

        _expansionDepth = expansionDepth;
        UseIconAndTypeName = useIconAndTypeName;

        Weight = 50.3f;
    }

    /// <summary>
    /// Lazy constructor where the object to navigate to is not fetched until the command is definetly for sure running (see <see cref="Execute"/>)
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="getObjectsFunc"></param>
    public ExecuteCommandShow(IBasicActivateItems activator,
        Func<IEnumerable<IMapsDirectlyToDatabaseTable>> getObjectsFunc) : base(activator)
    {
        _getObjectsFunc = getObjectsFunc;

        Weight = 50.3f;
    }

    public ExecuteCommandShow(IBasicActivateItems activator, int? foreignKey, Type typeToFetch) : base(activator)
    {
        if (!foreignKey.HasValue)
            SetImpossible("No object exists");

        _getObjectsFunc = () =>
            foreignKey.HasValue
                ? new IMapsDirectlyToDatabaseTable[]
                    { activator.RepositoryLocator.GetObjectByID(typeToFetch, foreignKey.Value) }
                : Array.Empty<IMapsDirectlyToDatabaseTable>();

        OverrideCommandName = $"{typeToFetch.Name}(s)";

        Weight = 50.3f;
    }

    public override string GetCommandName() =>
        !string.IsNullOrWhiteSpace(OverrideCommandName)
            ? base.GetCommandName()
            : UseIconAndTypeName && _objectType != null
                ? $"Show {_objectType.Name}(s)"
                : base.GetCommandName();

    public override void Execute()
    {
        FetchDestinationObjects();

        base.Execute();
        var show = _objectToShow;

        if (show == null && _objectsToPickFrom != null)
        {
            show = SelectOne(_objectsToPickFrom.Cast<DatabaseEntity>().ToList());

            if (show == null)
                return;
        }

        BasicActivator.RequestItemEmphasis(this, new EmphasiseRequest(show, _expansionDepth));
    }

    /// <summary>
    /// If late loading is setup for this command, this will execute the delegate code and update the command status to indicate whether there are any objects (and which objects) can be navigated to.  This method will be called automatically on Execute if not called before
    /// </summary>
    public void FetchDestinationObjects()
    {
        // If we have a lazy func to call when only on command execution, nows the time
        if (_getObjectsFunc != null && _objectsToPickFrom == null)
        {
            var pick = _getObjectsFunc()?.ToArray() ?? Array.Empty<IMapsDirectlyToDatabaseTable>();

            if (!pick.Any())
            {
                // base.Execute() will throw since the late load of objects returned nothing
                SetImpossible("No objects found");
            }
            else if (pick.Length == 1)
            {
                _objectToShow = pick[0];
            }
            else
            {
                _objectsToPickFrom = pick;
                _objectType = _objectsToPickFrom.First().GetType();
            }
        }
    }

    public override string GetCommandHelp() => "Opens the containing toolbox collection and shows the object";

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        OverrideIcon != null
            ? base.GetImage(iconProvider)
            : UseIconAndTypeName &&
              // if there is something to show
              (_objectType != null || _objectToShow != null)
                ?
                // return its icon
                iconProvider.GetImage((object)_objectToShow ?? _objectType)
                : null;

    /// <summary>
    /// Resolves any lamdas and returns what object(s) would be shown (if any)
    /// by running this command.  This method may be expensive to run
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IMapsDirectlyToDatabaseTable> GetObjects()
    {
        FetchDestinationObjects();
        return new[] { _objectToShow };
    }
}