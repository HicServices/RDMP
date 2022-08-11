// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using SixLabors.ImageSharp;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Icons.IconProvision;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders
{
    public class ProcessTaskStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private Image _exe;
        private Image _sql;
        private Image _plugin;
        private Image _attacher;
        private Image _dataProvider;
        private Image _mutilateDataTables;

        public ProcessTaskStateBasedIconProvider()
        {
            _exe = CatalogueIcons.Exe;
            _sql = CatalogueIcons.SQL;
            _plugin = CatalogueIcons.ProcessTask;

            _attacher = CatalogueIcons.Attacher;
            _dataProvider = CatalogueIcons.DataProvider;
            _mutilateDataTables = CatalogueIcons.MutilateDataTables;
        }

        public Image GetImageIfSupportedObject(object o)
        {
            var pt = o as ProcessTask;

            if(o is Type && o.Equals(typeof(ProcessTask)))
                return _plugin;

            if (pt == null)
                return null;

            switch (pt.ProcessTaskType)
            {
                case ProcessTaskType.Executable:
                    return _exe;
                case ProcessTaskType.SQLFile:
                    return _sql;
                case ProcessTaskType.Attacher:
                    return _attacher;
                case ProcessTaskType.DataProvider:
                    return _dataProvider;
                case ProcessTaskType.MutilateDataTable:
                    return _mutilateDataTables;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}