// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandShow : BasicCommandExecution, IAtomicCommand
    {
        private IMapsDirectlyToDatabaseTable _objectToShow;
        /// <summary>
        /// Alternative to <see cref="_objectToShow"/> where there might be many objects and it could be expensive to fetch them so only do so when command is executed
        /// </summary>
        private Func<IEnumerable<IMapsDirectlyToDatabaseTable>> _getObjectsFunc;
        private IMapsDirectlyToDatabaseTable[] _objectsToPickFrom;


        private readonly int _expansionDepth;
        private readonly bool _useIconAndTypeName;
        private readonly Type _objectType;

        public ExecuteCommandShow(IBasicActivateItems activator, IMapsDirectlyToDatabaseTable objectToShow, int expansionDepth, bool useIconAndTypeName = false) : base(activator)
        {
            _objectToShow = objectToShow;
            _objectType = _objectToShow?.GetType();
            _expansionDepth = expansionDepth;
            _useIconAndTypeName = useIconAndTypeName;

            if (_objectToShow == null)
                SetImpossible("No objects found");
        }


        public ExecuteCommandShow(IBasicActivateItems activator, IEnumerable<IMapsDirectlyToDatabaseTable> objectsToPickFrom, int expansionDepth, bool useIconAndTypeName = false) : base(activator)
        {
            if (objectsToPickFrom == null)
            {
                SetImpossible("No objects found");
                return;
            }

            var obs = objectsToPickFrom.ToArray();

            //no objects!
            if (obs.Length == 0)
                SetImpossible("No objects found");
            else
            if (obs.Length == 1)
            {
                //one object only
                _objectToShow = objectsToPickFrom.Single();
                _objectType = _objectToShow.GetType();
            }
            else
            {
                //many objects, lets assume they are of the same type for display purposes
                _objectType = objectsToPickFrom.First().GetType();
                _objectsToPickFrom = obs;
            }

            _expansionDepth = expansionDepth;
            _useIconAndTypeName = useIconAndTypeName;
        }

        /// <summary>
        /// Lazy constructor where the object to navigate to is not fetched until the command is definetly for sure running (see <see cref="Execute"/>)
        /// </summary>
        /// <param name="activator"></param>
        /// <param name="getObjectsFunc"></param>
        public ExecuteCommandShow(IBasicActivateItems activator, Func<IEnumerable<IMapsDirectlyToDatabaseTable>> getObjectsFunc) : base(activator)
        {
            _getObjectsFunc = getObjectsFunc;
        }

        public ExecuteCommandShow(IBasicActivateItems activator, int? foreignKey, Type typeToFetch):base(activator)
        {
            if(!foreignKey.HasValue)
                SetImpossible("No object exists");

            _getObjectsFunc = ()=> foreignKey.HasValue ?
                new IMapsDirectlyToDatabaseTable[]{activator.RepositoryLocator.GetObjectByID(typeToFetch,foreignKey.Value) } :
                new IMapsDirectlyToDatabaseTable[0];

            OverrideCommandName = typeToFetch.Name;
        }

        public override string GetCommandName()
        {
            return _useIconAndTypeName && _objectType != null ? "Show " + _objectType.Name : base.GetCommandName();
        }

        public override void Execute()
        {
            FetchDestinationObjects();

            base.Execute();

            if (_objectToShow == null && _objectsToPickFrom != null)
            {
                _objectToShow = SelectOne(_objectsToPickFrom.Cast<DatabaseEntity>().ToList());

                if (_objectToShow == null)
                    return;
            }
            BasicActivator.RequestItemEmphasis(this,new EmphasiseRequest(_objectToShow, _expansionDepth));
        }

        /// <summary>
        /// If late loading is setup for this command, this will execute the delegate code and update the command status to indicate whether there are any objects (and which objects) can be navigated to.  This method will be called automatically on Execute if not called before
        /// </summary>
        public void FetchDestinationObjects()
        {
            // If we have a lazy func to call when only on command execution, nows the time
            if(_getObjectsFunc != null && _objectsToPickFrom == null)
            {
                var pick = _getObjectsFunc()?.ToArray() ?? new IMapsDirectlyToDatabaseTable[0];

                if (!pick.Any())
                {
                    // base.Execute() will throw since the late load of objects returned nothing
                    SetImpossible("No objects found");
                }
                else if(pick.Length == 1)
                {
                    _objectToShow = pick[0];
                }
                else
                {
                    _objectsToPickFrom = pick;
                }
            }

        }

        public override string GetCommandHelp()
        {
            return "Opens the containing toolbox collection and shows the object";
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return _useIconAndTypeName && _objectType != null ? iconProvider.GetImage((object)_objectToShow ?? _objectType) : null;
        }
    }
}