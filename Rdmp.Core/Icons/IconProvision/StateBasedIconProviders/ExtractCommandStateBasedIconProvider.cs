// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using SixLabors.ImageSharp;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.Icons.IconProvision;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders
{
    public class ExtractCommandStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private Image _waiting;
        private Image _warning;
        private Image _writing;
        private Image _failed;
        private Image _tick;

        public ExtractCommandStateBasedIconProvider()
        {
            _waiting = CatalogueIcons.Waiting;
            _warning = CatalogueIcons.Warning;
            _writing = CatalogueIcons.Writing;
            _failed = CatalogueIcons.Failed;
            _tick = CatalogueIcons.Tick;
        }
        public Image GetImageIfSupportedObject(object o)
        {
            if (!(o is ExtractCommandState))
                return null;

            var ecs = (ExtractCommandState) o;

            switch (ecs)
            {
                case ExtractCommandState.NotLaunched:
                    return _waiting;
                case ExtractCommandState.WaitingForSQLServer:
                    return _waiting;
                case ExtractCommandState.WritingToFile:
                    return _writing;
                case ExtractCommandState.Crashed:
                    return _failed;
                case ExtractCommandState.UserAborted:
                    return _failed;
                case ExtractCommandState.Completed:
                    return _tick;
                case ExtractCommandState.Warning:
                    return _warning;
                case ExtractCommandState.WritingMetadata:
                    return _writing;
                case ExtractCommandState.WaitingToExecute:
                    return _waiting;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}