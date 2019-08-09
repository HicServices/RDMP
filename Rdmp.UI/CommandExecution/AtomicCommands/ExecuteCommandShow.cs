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
using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.ItemActivation.Emphasis;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandShow : BasicUICommandExecution,IAtomicCommand
    {
        private IMapsDirectlyToDatabaseTable _objectToShow;
        private readonly IMapsDirectlyToDatabaseTable[] _objectsToPickFrom;


        private readonly int _expansionDepth;
        private readonly bool _useIconAndTypeName;
        private readonly Type _objectType;

        public ExecuteCommandShow(IActivateItems activator, IMapsDirectlyToDatabaseTable objectToShow, int expansionDepth, bool useIconAndTypeName=false):base(activator)
        {
            _objectToShow = objectToShow;
            _objectType = _objectToShow.GetType();
            _expansionDepth = expansionDepth;
            _useIconAndTypeName = useIconAndTypeName;
        }


        public ExecuteCommandShow(IActivateItems activator, IEnumerable<IMapsDirectlyToDatabaseTable> objectsToPickFrom, int expansionDepth, bool useIconAndTypeName=false) :base(activator) 
        {
            if(objectsToPickFrom == null)
            {
                SetImpossible("No objects found");
                return;
            }

            var obs  =  objectsToPickFrom.ToArray();

            //no objects!
            if(obs.Length == 0)
                SetImpossible("No objects found");
            else
            if(obs.Length == 1)
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


        public override string GetCommandName()
        {
            return _useIconAndTypeName && _objectType != null? "Show " + _objectType.Name :base.GetCommandName(); 
        }

        public override void Execute()
        {
            base.Execute();

            if(_objectToShow == null && _objectsToPickFrom != null)
            {
                _objectToShow = SelectOne(_objectsToPickFrom.Cast<DatabaseEntity>().ToList());

                if(_objectToShow == null)
                    return;
            }

            Activator.RequestItemEmphasis(this, new EmphasiseRequest(_objectToShow,_expansionDepth));
        }

        public override string GetCommandHelp()
        {
            return "Opens the containing toolbox collection and shows the object";
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return _useIconAndTypeName && _objectType != null? iconProvider.GetImage((object)_objectToShow??_objectType):null;
        }
    }
}